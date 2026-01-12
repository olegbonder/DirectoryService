using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations.Commands.SoftDeleteLocation
{
    public sealed class SoftDeleteLocationHandler : ICommandHandler<Guid, SoftDeleteLocationCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IValidator<SoftDeleteLocationCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<SoftDeleteLocationHandler> _logger;

        public SoftDeleteLocationHandler(
            ITransactionManager transactionManager,
            ILocationsRepository locationsRepository,
            IValidator<SoftDeleteLocationCommand> validator,
            ICacheService cache,
            ILogger<SoftDeleteLocationHandler> logger)
        {
            _transactionManager = transactionManager;
            _locationsRepository = locationsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(SoftDeleteLocationCommand command, CancellationToken cancellationToken)
        {
            var validResult = await _validator.ValidateAsync(command, cancellationToken);
            if (validResult.IsValid == false)
            {
                return validResult.ToList();
            }

            var transactionScopeResult = await _transactionManager.BeginTransaction(cancellationToken: cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Errors;
            }

            using var transactionScope = transactionScopeResult.Value;

            var locId = command.LocationId;
            var locationId = LocationId.Current(locId);

            var existingLocationResult = await _locationsRepository.GetActiveLocationsByIds(new List<LocationId> { locationId }, cancellationToken);
            if (existingLocationResult.IsFailure)
            {
                transactionScope.RollBack();
                return existingLocationResult.Errors!;
            }

            var existingLocation = existingLocationResult.Value.FirstOrDefault();
            if (existingLocation == null)
            {
                transactionScope.RollBack();
                return LocationErrors.NotFound(locId);
            }

            var locationResult = await _locationsRepository.DeactivateLocation(locationId, cancellationToken);
            if (locationResult.IsFailure)
            {
                transactionScope.RollBack();
                return locationResult.Errors;
            }

            try
            {
                await _transactionManager.SaveChanges(cancellationToken);
            }
            catch (Exception ex)
            {
                transactionScope.RollBack();
                _logger.LogError(ex, "Ошибка обновления локации с {id}", locId);
                return LocationErrors.DatabaseUpdateError(locId);
            }

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_LOCATION_KEY, cancellationToken);

            _logger.LogInformation("Локация с id = {id} не активна", locId);

            return locId;
        }
    }
}

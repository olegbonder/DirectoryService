using Core.Abstractions;
using Core.Caching;
using Core.Validation;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

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

            var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionResult.IsFailure)
            {
                return transactionResult.Errors;
            }

            var locId = command.LocationId;
            var locationId = LocationId.Current(locId);

            var existingLocationResult = await _locationsRepository
                .GetActiveLocationsByIds([locationId], cancellationToken);
            if (existingLocationResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return existingLocationResult.Errors;
            }

            var existingLocation = existingLocationResult.Value.FirstOrDefault();
            if (existingLocation == null)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return LocationErrors.NotFound(locId);
            }

            var locationResult = await _locationsRepository.DeactivateLocation(locationId, cancellationToken);
            if (locationResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return locationResult.Errors;
            }

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                _logger.LogError("Ошибка обновления локации с {id}", locId);
                return LocationErrors.DatabaseUpdateError(locId);
            }

            var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
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

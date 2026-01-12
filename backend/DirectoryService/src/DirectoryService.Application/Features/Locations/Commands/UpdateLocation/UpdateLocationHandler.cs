using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations.Commands.UpdateLocation
{
    public sealed class UpdateLocationHandler : ICommandHandler<Guid, UpdateLocationCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IValidator<UpdateLocationCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateLocationHandler> _logger;

        public UpdateLocationHandler(
            ITransactionManager transactionManager,
            ILocationsRepository locationsRepository,
            IValidator<UpdateLocationCommand> validator,
            ICacheService cache,
            ILogger<UpdateLocationHandler> logger)
        {
            _transactionManager = transactionManager;
            _locationsRepository = locationsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
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

            var locName = LocationName.Create(command.Request.Name).Value;

            var locAdr = command.Request.Address;

            var locAddress = LocationAddress.Create(locAdr.Country, locAdr.City, locAdr.Street, locAdr.House, locAdr.Flat).Value;

            var locTimeZone = LocationTimezone.Create(command.Request.TimeZone).Value;

            var locationResult = Location.Update(locationId, locName, locAddress, locTimeZone);
            if (locationResult.IsFailure)
            {
                transactionScope.RollBack();
                return locationResult.Errors!;
            }

            var location = locationResult.Value;
            var updlocationResult = await _locationsRepository.UpdateLocation(location, cancellationToken);
            if (updlocationResult.IsFailure)
            {
                transactionScope.RollBack();
                return updlocationResult.Errors;
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

            _logger.LogInformation("Локация с id = {id} обновлена", locId);

            return locId;
        }
    }
}

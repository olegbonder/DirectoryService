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

            var existingLocation = await _locationsRepository.GetActiveLocationById(locationId, cancellationToken);
            if (existingLocation == null)
            {
                transactionScope.RollBack();
                return LocationErrors.NotFound(locId);
            }

            var request = command.Request;
            var name = request.Name;
            var locName = LocationName.Create(name).Value;

            var locAdr = request.Address;

            var locAddress = LocationAddress.Create(locAdr.Country, locAdr.City, locAdr.Street, locAdr.House, locAdr.Flat).Value;

            var locTimeZone = LocationTimezone.Create(request.TimeZone).Value;

            existingLocation.Update(locName, locAddress, locTimeZone);
            var updateResult = await _locationsRepository.Update(existingLocation, cancellationToken);
            if (updateResult.IsFailure)
            {
                transactionScope.RollBack();
                return updateResult.Errors;
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

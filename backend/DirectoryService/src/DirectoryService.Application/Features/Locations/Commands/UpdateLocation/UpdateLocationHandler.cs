using Core.Abstractions;
using Core.Caching;
using Core.Validation;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

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

            var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionResult.IsFailure)
            {
                return transactionResult.Errors;
            }

            var locId = command.LocationId;
            var locationId = LocationId.Current(locId);

            var existingLocation = await _locationsRepository.GetActiveLocationById(locationId, cancellationToken);
            if (existingLocation == null)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return LocationErrors.NotFound(locId);
            }

            var request = command.Request;
            string name = request.Name;
            var locName = LocationName.Create(name).Value;

            var locAdr = request.Address;

            var locAddress = LocationAddress.Create(locAdr.Country, locAdr.City, locAdr.Street, locAdr.House, locAdr.Flat).Value;

            var locTimeZone = LocationTimezone.Create(request.TimeZone).Value;

            existingLocation.Update(locName, locAddress, locTimeZone);
            var updateResult = await _locationsRepository.Update(existingLocation, cancellationToken);
            if (updateResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return updateResult.Errors;
            }

            var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
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

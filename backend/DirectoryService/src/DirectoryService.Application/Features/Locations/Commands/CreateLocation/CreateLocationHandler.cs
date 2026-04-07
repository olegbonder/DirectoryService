using Core.Abstractions;
using Core.Caching;
using Core.Validation;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Locations.Commands.CreateLocation
{
    public sealed class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly ILocationsRepository _locationsRepository;
        private readonly ICacheService _cache;
        private readonly IValidator<CreateLocationCommand> _validator;
        private readonly ILogger<CreateLocationHandler> _logger;

        public CreateLocationHandler(
            ITransactionManager transactionManager,
            ILocationsRepository locationsRepository,
            ICacheService cache,
            IValidator<CreateLocationCommand> validator,
            ILogger<CreateLocationHandler> logger)
        {
            _transactionManager = transactionManager;
            _locationsRepository = locationsRepository;
            _cache = cache;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
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

            var locName = LocationName.Create(command.Request.Name).Value;

            var locAdr = command.Request.Address;

            var locAddress = LocationAddress.Create(locAdr.Country, locAdr.City, locAdr.Street, locAdr.House, locAdr.Flat).Value;

            var locTimeZone = LocationTimezone.Create(command.Request.TimeZone).Value;

            var locationResult = Location.Create(locName, locAddress, locTimeZone);
            if (locationResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return locationResult.Errors;
            }

            // Бизнес валидация
            var addLocationResult = await _locationsRepository.Add(locationResult.Value, cancellationToken);
            if (addLocationResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return addLocationResult.Errors;
            }

            var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_LOCATION_KEY, cancellationToken);

            _logger.LogInformation("Локация с id = {id} сохранена в БД", addLocationResult.Value);

            return addLocationResult.Value;
        }
    }
}

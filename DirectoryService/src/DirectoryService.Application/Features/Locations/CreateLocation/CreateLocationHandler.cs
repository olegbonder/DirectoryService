using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Features.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations.CreateLocation
{
    public sealed class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly IValidator<CreateLocationCommand> _validator;
        private readonly ILogger<CreateLocationHandler> _logger;

        public CreateLocationHandler(
            ILocationsRepository locationsRepository,
            IValidator<CreateLocationCommand> validator,
            ILogger<CreateLocationHandler> logger)
        {
            _locationsRepository = locationsRepository;
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

            var locName = LocationName.Create(command.Request.Name).Value;

            var locAdr = command.Request.Address;

            var locAddress = LocationAddress.Create(locAdr.Country, locAdr.City, locAdr.Street, locAdr.House, locAdr.FlatNumber).Value;

            var locTimeZone = LocationTimezone.Create(command.Request.TimeZone).Value;

            var locationRes = Location.Create(locName, locAddress, locTimeZone);
            if (locationRes.IsFailure)
            {
                return locationRes.Errors!;
            }

            // Бизнес валидация
            var addLocationRes = await _locationsRepository.AddAsync(locationRes.Value, cancellationToken);
            if (addLocationRes.IsFailure)
            {
                return addLocationRes.Errors!;
            }

            _logger.LogInformation("Location с {id} добавлен", addLocationRes.Value);

            return addLocationRes.Value;
        }
    }
}

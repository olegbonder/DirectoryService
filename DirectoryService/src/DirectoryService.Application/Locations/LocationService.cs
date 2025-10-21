using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations
{
    public class LocationService : ILocationService
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly ILogger<LocationService> _logger;

        public LocationService(ILocationsRepository locationsRepository, ILogger<LocationService> logger)
        {
            _locationsRepository = locationsRepository;
            _logger = logger;
        }

        public async Task<Result<Guid>> Create(CreateLocationDTO locationDTO, CancellationToken cancellationToken)
        {
            var locNameRes = LocationName.Create(locationDTO.Name);
            if (locNameRes.IsFailure)
            {
                return locNameRes.Error!;
            }

            var locAdr = locationDTO.Address;
            if (locAdr == null)
            {
                return "Адрес не может быть пустым";
            }

            var locAddressRes = LocationAddress.Create(locAdr.Country, locAdr.City, locAdr.Street, locAdr.House, locAdr.FlatNumber);
            if (locAddressRes.IsFailure)
            {
                return locAddressRes.Error!;
            }

            var locTimeZoneRes = LocationTimezone.Create(locationDTO.TimeZone);
            if (locTimeZoneRes.IsFailure)
            {
                return locTimeZoneRes.Error!;
            }

            var locationRes = Location.Create(locNameRes.Value, locAddressRes.Value, locTimeZoneRes.Value);
            if (locationRes.IsFailure)
            {
                return locationRes.Error!;
            }

            var addLocationRes = await _locationsRepository.AddAsync(locationRes.Value, cancellationToken);
            if (addLocationRes.IsFailure)
            {
                return addLocationRes.Error!;
            }

            _logger.LogInformation("Location с {id} добавлен", addLocationRes.Value);

            return addLocationRes.Value;
        }
    }
}

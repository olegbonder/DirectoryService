using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations
{
    public class LocationAddress
    {
        private LocationAddress(AddressDTO dto)
        {
            Country = dto.Country;
            City = dto.City;
            Street = dto.Street;
            HouseNumber = dto.House;
            FlatNumber = dto.FlatNumber;
        }

        public string FullAddress => $"{Country} {City} {Street} {HouseNumber} {FlatNumber}";

        public string Country { get; }

        public string City { get; }

        public string Street { get; }

        public string HouseNumber { get; }

        public string? FlatNumber { get; }

        public static Result<LocationAddress> Create(AddressDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Country))
            {
                return $"Свойство \"{nameof(Country)}\" не должно быть пустым";
            }

            if (string.IsNullOrWhiteSpace(dto.City))
            {
                return $"Свойство \"{nameof(City)}\" не должно быть пустым";
            }

            if (string.IsNullOrWhiteSpace(dto.Street))
            {
                return $"Свойство \"{nameof(Street)}\" не должно быть пустым";
            }

            if (string.IsNullOrWhiteSpace(dto.House))
            {
                return $"Свойство \"{nameof(HouseNumber)}\" не должно быть пустым";
            }

            return new LocationAddress(dto);
        }
    }
}

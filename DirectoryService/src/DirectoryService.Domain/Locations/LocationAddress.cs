using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations
{
    public class LocationAddress
    {
        private LocationAddress(string country, string city, string street, string houseNumber, string? flatNumber)
        {
            Country = country;
            City = city;
            Street = street;
            HouseNumber = houseNumber;
            FlatNumber = flatNumber;
        }

        public string FullAddress => $"{Country} {City} {Street} {HouseNumber} {FlatNumber}";

        public string Country { get; }

        public string City { get; }

        public string Street { get; }

        public string HouseNumber { get; }

        public string? FlatNumber { get; }

        public static Result<LocationAddress> Create(string country, string city, string street, string houseNumber, string? flatNumber)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return $"Свойство \"{nameof(Country)}\" не должно быть пустым";
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                return $"Свойство \"{nameof(City)}\" не должно быть пустым";
            }

            if (string.IsNullOrWhiteSpace(street))
            {
                return $"Свойство \"{nameof(Street)}\" не должно быть пустым";
            }

            if (string.IsNullOrWhiteSpace(houseNumber))
            {
                return $"Свойство \"{nameof(HouseNumber)}\" не должно быть пустым";
            }

            return new LocationAddress(country, city, street, houseNumber, flatNumber);
        }
    }
}

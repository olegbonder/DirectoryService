using Shared.Result;

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
                return GeneralErrors.PropertyIsEmpty("location.address.country", "Страна");
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                return GeneralErrors.PropertyIsEmpty("location.address.city", "Город");
            }

            if (string.IsNullOrWhiteSpace(street))
            {
                return GeneralErrors.PropertyIsEmpty("location.address.street", "Улица");
            }

            if (string.IsNullOrWhiteSpace(houseNumber))
            {
                return GeneralErrors.PropertyIsEmpty("location.address.house", "Дом");
            }

            return new LocationAddress(country, city, street, houseNumber, flatNumber);
        }
    }
}

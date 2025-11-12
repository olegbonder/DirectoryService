using Shared.Result;

namespace DirectoryService.Domain.Locations
{
    public sealed record LocationAddress
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
            var errors = new List<Error>();
            if (string.IsNullOrWhiteSpace(country))
            {
                errors.Add(GeneralErrors.PropertyIsEmpty("location.address.country", "Страна"));
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                errors.Add(GeneralErrors.PropertyIsEmpty("location.address.city", "Город"));
            }

            if (string.IsNullOrWhiteSpace(street))
            {
                errors.Add(GeneralErrors.PropertyIsEmpty("location.address.street", "Улица"));
            }

            if (string.IsNullOrWhiteSpace(houseNumber))
            {
                errors.Add(GeneralErrors.PropertyIsEmpty("location.address.house", "Дом"));
            }

            if (errors.Any())
            {
                return new Errors(errors);
            }

            return new LocationAddress(country, city, street, houseNumber, flatNumber);
        }
    }
}

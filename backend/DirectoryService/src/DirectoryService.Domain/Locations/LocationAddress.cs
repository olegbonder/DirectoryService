using DirectoryService.Domain.Shared;
using SharedKernel.Result;

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

        public static Result<LocationAddress> Create(string country, string city, string street, string houseNumber, string? flatNumber = null)
        {
            var errors = new List<Error>();
            if (string.IsNullOrWhiteSpace(country))
            {
                errors.Add(LocationErrors.CountryIsEmpty());
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                errors.Add(LocationErrors.CityIsEmpty());
            }

            if (string.IsNullOrWhiteSpace(street))
            {
                errors.Add(LocationErrors.StreetIsEmpty());
            }

            if (string.IsNullOrWhiteSpace(houseNumber))
            {
                errors.Add(LocationErrors.HouseNumberIsEmpty());
            }

            if (errors.Any())
            {
                return new Errors(errors);
            }

            return new LocationAddress(country, city, street, houseNumber, flatNumber);
        }
    }
}

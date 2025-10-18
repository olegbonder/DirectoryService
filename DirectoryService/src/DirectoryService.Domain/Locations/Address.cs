using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations
{
    public class Address
    {
        private Address(string country, string city, string street, string houseNumber, string? flatNumber)
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

        public static Result<Address> Create(string country, string city, string street, string houseNumber, string? flatNumber)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return Result.Failure<Address>($"Свойство \"{nameof(Country)}\" не должно быть пустым");
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                return Result.Failure<Address>($"Свойство \"{nameof(City)}\" не должно быть пустым");
            }

            if (string.IsNullOrWhiteSpace(street))
            {
                return Result.Failure<Address>($"Свойство \"{nameof(Street)}\" не должно быть пустым");
            }

            if (string.IsNullOrWhiteSpace(houseNumber))
            {
                return Result.Failure<Address>($"Свойство \"{nameof(HouseNumber)}\" не должно быть пустым");
            }

            return new Address(country, city, street, houseNumber, flatNumber);
        }
    }
}

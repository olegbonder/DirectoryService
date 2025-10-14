namespace DirectoryService.Domain.Location
{
    public class Address
    {
        public Address(string country, string city, string street, string houseNumber, string? flatNumber)
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
    }
}

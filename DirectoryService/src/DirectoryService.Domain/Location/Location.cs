namespace DirectoryService.Domain.Location
{
    public class Location
    {
        public Location(LocationName name, Address address, LocationTimezone timezone)
        {
            Id = Guid.NewGuid();
            Name = name;
            Address = address;
            Timezone = timezone;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }

        public LocationName Name { get; private set; }

        public Address Address { get; private set; }

        public LocationTimezone Timezone { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }
    }
}

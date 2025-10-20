using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations
{
    public class Location : Entity<LocationId>
    {
        // EF Core
        private Location(LocationId id)
            : base(id)
        {
        }

        private Location(LocationId id, LocationName name, Address address, LocationTimezone timezone)
            : base(id)
        {
            Name = name;
            Address = address;
            Timezone = timezone;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public LocationName Name { get; private set; }

        public Address Address { get; private set; }

        public LocationTimezone Timezone { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public static Result<Location> Create(LocationId id, LocationName name, Address address, LocationTimezone timezone)
        {
            return new Location(id, name, address, timezone);
        }
    }
}

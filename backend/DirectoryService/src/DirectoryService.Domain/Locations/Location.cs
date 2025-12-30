using DirectoryService.Domain.Shared;
using Shared.Result;

namespace DirectoryService.Domain.Locations
{
    public sealed class Location : Entity<LocationId>
    {
        // EF Core
        private Location(LocationId id)
            : base(id)
        {
        }

        private Location(LocationId id, LocationName name, LocationAddress address, LocationTimezone timezone)
            : base(id)
        {
            Name = name;
            Address = address;
            Timezone = timezone;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public LocationName Name { get; private set; } = null!;

        public LocationAddress Address { get; private set; } = null!;

        public LocationTimezone Timezone { get; private set; } = null!;

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public static Result<Location> Create(LocationName name, LocationAddress address, LocationTimezone timezone)
        {
            var newLocationId = LocationId.Create();
            return new Location(newLocationId, name, address, timezone);
        }

        public void SoftDelete()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

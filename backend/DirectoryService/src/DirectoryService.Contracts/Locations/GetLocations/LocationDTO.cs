namespace DirectoryService.Contracts.Locations.GetLocations
{
    public record LocationDTO
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;

        public string Country { get; init; } = null!;

        public string City { get; init; } = null!;

        public string Street { get; init; } = null!;

        public string House { get; init; } = null!;

        public string? Flat { get; init; }

        public string TimeZone { get; init; } = null!;

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }

    }
}

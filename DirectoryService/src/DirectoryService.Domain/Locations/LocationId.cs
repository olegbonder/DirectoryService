namespace DirectoryService.Domain.Locations
{
    public record LocationId
    {
        private LocationId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; } = Guid.Empty;

        public static LocationId Create() => new(Guid.NewGuid());
        public static LocationId Current(Guid id) => new(id);
    }
}

namespace DirectoryService.Domain.Positions
{
    public record PositionId
    {
        private PositionId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; } = Guid.Empty;

        public static PositionId Create() => new(Guid.NewGuid());
        public static PositionId Current(Guid id) => new(id);
    }
}

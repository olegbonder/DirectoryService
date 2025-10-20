using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions
{
    public class Position : Entity<PositionId>
    {
        // EF Core
        private Position(PositionId id)
            : base(id)
        {
        }

        private Position(PositionId id,  PositionName name, PositionDesription desription)
            : base(id)
        {
            Name = name;
            Description = desription;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public PositionName Name { get; private set; }

        public PositionDesription Description { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public static Result<Position> Create(PositionId id, PositionName name, PositionDesription desription)
        {
            return new Position(id, name, desription);
        }
    }
}

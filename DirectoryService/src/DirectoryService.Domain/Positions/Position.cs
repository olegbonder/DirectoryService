using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions
{
    public class Position
    {
        // EF Core
        private Position()
        {
        }

        private Position(PositionName name, PositionDesription desription)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = desription;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }

        public PositionName Name { get; private set; }

        public PositionDesription Description { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public static Result<Position> Create(PositionName name, PositionDesription desription)
        {
            return new Position(name, desription);
        }
    }
}

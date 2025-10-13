using DirectoryService.Domain.Location;

namespace DirectoryService.Domain.Position
{
    public class Position
    {
        public Position(PositionName name, PositionDesription desription)
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
    }
}

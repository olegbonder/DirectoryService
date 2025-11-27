using DirectoryService.Domain.Shared;
using Shared.Result;

namespace DirectoryService.Domain.Positions
{
    public class Position : Entity<PositionId>
    {
        private List<DepartmentPosition> _departments = [];

        // EF Core
        private Position(PositionId id)
            : base(id)
        {
        }

        private Position(
            PositionId id,
            PositionName name,
            PositionDesription desription,
            IReadOnlyCollection<DepartmentPosition> departmentPositions)
            : base(id)
        {
            Name = name;
            Description = desription;
            _departments = departmentPositions.ToList();
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public PositionName Name { get; private set; } = null!;

        public PositionDesription Description { get; private set; } = null!;

        public List<DepartmentPosition> DepartmentPositions => _departments;

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public static Result<Position> Create(
            PositionId id,
            PositionName name,
            PositionDesription desription,
            IReadOnlyCollection<DepartmentPosition> departmentPositions)
        {
            if (departmentPositions.Count < 1)
            {
                return PositionErrors.PositionMustHaveMoreOneDepartment();
            }

            return new Position(id, name, desription, departmentPositions);
        }
    }
}

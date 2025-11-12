using DirectoryService.Domain.Shared;
using Shared.Result;

namespace DirectoryService.Domain.Departments
{
    public sealed class Department : Entity<DepartmentId>
    {
        private IReadOnlyCollection<Department> _children = [];
        private IReadOnlyCollection<DepartmentLocation> _locations = [];
        private IReadOnlyCollection<DepartmentPosition> _positions = [];

        // EF Core
        private Department(DepartmentId id)
            : base(id)
        {
        }

        private Department(
            DepartmentId id,
            DepartmentName name,
            DepartmentIdentifier identifier,
            DepartmentId parentId,
            IEnumerable<Department> childs,
            DepartmentPath path,
            IReadOnlyCollection<DepartmentLocation> locations,
            IReadOnlyCollection<DepartmentPosition> positions)
            : base(id)
        {
            Name = name;
            Identifier = identifier;
            ParentId = parentId;
            _children = childs.ToList();
            _locations = locations;
            _positions = positions;
            Path = path;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public DepartmentName Name { get; private set; } = null!;

        public DepartmentIdentifier Identifier { get; private set; } = null!;

        public DepartmentId? ParentId { get; private set; } = null!;

        public IReadOnlyCollection<Department> Children => _children;

        public IReadOnlyCollection<DepartmentLocation> DepartmentLocations => _locations;

        public IReadOnlyCollection<DepartmentPosition> DepartmentPositions => _positions;

        public DepartmentPath Path { get; private set; } = null!;

        public short Depth { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public Result<Department> Create(
            DepartmentName name,
            DepartmentIdentifier identifier,
            DepartmentId parentId,
            IEnumerable<Department> childs,
            DepartmentPath path,
            IReadOnlyCollection<DepartmentLocation> locations,
            IReadOnlyCollection<DepartmentPosition> positions)
        {
            if (locations.Count < 1)
            {
                return Error.Validation("department.has.not.locations", "У подразделения должна быть хотя бы одна локация");
            }

            var newDeptId = DepartmentId.Create();
            return new Department(newDeptId, name, identifier, parentId, childs, path, locations, positions);
        }
    }
}

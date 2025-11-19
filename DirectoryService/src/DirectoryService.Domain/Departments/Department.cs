using DirectoryService.Domain.Shared;
using Shared.Result;

namespace DirectoryService.Domain.Departments
{
    public sealed class Department : Entity<DepartmentId>
    {
        private List<Department> _children = [];
        private IReadOnlyCollection<DepartmentLocation> _locations = [];

        // EF Core
        private Department(DepartmentId id)
            : base(id)
        {
        }

        private Department(
            DepartmentId id,
            DepartmentId? parentId,
            DepartmentName name,
            DepartmentIdentifier identifier,
            DepartmentPath path,
            int depth,
            IReadOnlyCollection<DepartmentLocation> locations)
            : base(id)
        {
            Name = name;
            Identifier = identifier;
            ParentId = parentId;
            _locations = locations;
            Path = path;
            Depth = depth;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public DepartmentName Name { get; private set; } = null!;

        public DepartmentIdentifier Identifier { get; private set; } = null!;

        public DepartmentId? ParentId { get; private set; } = null!;

        public List<Department> Children => _children;

        public IReadOnlyCollection<DepartmentLocation> DepartmentLocations => _locations;

        public DepartmentPath Path { get; private set; } = null!;

        public int Depth { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public static Result<Department> Create(
            DepartmentId id,
            DepartmentId? parentId,
            DepartmentName name,
            DepartmentIdentifier identifier,
            DepartmentPath path,
            int depth,
            IReadOnlyCollection<DepartmentLocation> locations)
        {
            if (locations.Count < 1)
            {
                return DepartmentErrors.DepartmentMustHaveMoreOneLocation();
            }

            return new Department(id, parentId, name, identifier, path, depth, locations);
        }

        public Result UpdateLocations(IReadOnlyCollection<DepartmentLocation> locations)
        {
            if (locations.Count < 1)
            {
                return DepartmentErrors.DepartmentMustHaveMoreOneLocation();
            }

            _locations = locations;

            return Result.Success();
        }
    }
}

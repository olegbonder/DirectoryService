using DirectoryService.Domain.Shared;
using Shared.Result;

namespace DirectoryService.Domain.Departments
{
    public sealed class Department : Entity<DepartmentId>
    {
        private List<Department> _children = [];
        private List<DepartmentLocation> _locations = [];

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
            _locations = locations.ToList();
            Path = path;
            Depth = depth;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public DepartmentName Name { get; private set; } = null!;

        public DepartmentIdentifier Identifier { get; private set; } = null!;

        public DepartmentId? ParentId { get; private set; } = null!;

        public List<Department> Children => _children;

        public List<DepartmentLocation> DepartmentLocations => _locations;

        public DepartmentPath Path { get; private set; } = null!;

        public int Depth { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; }

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

        public void MoveDepartment(Department? newParentDepartment)
        {
            if (newParentDepartment == null)
            {
                Depth = 0;
                Path = DepartmentPath.Create(Identifier);
            }
            else
            {
                Depth = newParentDepartment.Depth + 1;
                Path = DepartmentPath.Create(Identifier, newParentDepartment);
            }

            ParentId = newParentDepartment?.Id;
            UpdatedAt = DateTime.UtcNow;
        }

        public Result UpdateLocations(IEnumerable<DepartmentLocation> locations)
        {
            if (locations.Count() < 1)
            {
                return DepartmentErrors.DepartmentMustHaveMoreOneLocation();
            }

            _locations = locations.ToList();

            return Result.Success();
        }

        public void SoftDelete()
        {
            IsActive = false;
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DeletedAt.Value;
            Path = DepartmentPath.CreateForSoftDelete(Identifier);
        }

        public void Update(DepartmentId? parentDepartmentId, DepartmentName deptName, DepartmentIdentifier deptIdentifier, DepartmentPath deptPath, int depth, List<DepartmentLocation> locationDepartments)
        {
            ParentId = parentDepartmentId;
            Name = deptName;
            Identifier = deptIdentifier;
            Path = deptPath;
            Depth = depth;
            _locations = locationDepartments;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

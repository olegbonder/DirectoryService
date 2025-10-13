namespace DirectoryService.Domain.Department
{
    public class Department
    {
        private IReadOnlyCollection<Department> _childDepartments = [];

        public Department(DepartmentName name, DepartmentIdentifier identifier, Guid parentId, IEnumerable<Department> childs, DepartmentPath path)
        {
            Id = Guid.NewGuid();
            Name = name;
            Identifier = identifier;
            ParentId = parentId;
            _childDepartments = childs.ToList();
            Path = path;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }

        public DepartmentName Name { get; private set; }

        public DepartmentIdentifier Identifier { get; private set; }

        public Guid? ParentId { get; private set; }

        public IReadOnlyCollection<Department> ChildDepartments => _childDepartments;

        public DepartmentPath Path { get; private set; }

        public short Depth { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }
    }
}

namespace DirectoryService.Contracts.Departments.GetDepartments
{
    public record DepartmentDTO
    {
        public Guid Id { get; init; }

        public Guid? ParentId { get; init; }

        public string Name { get; init; } = null!;

        public string Identifier { get; init; } = null!;

        public string Path { get; init; } = null!;

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}

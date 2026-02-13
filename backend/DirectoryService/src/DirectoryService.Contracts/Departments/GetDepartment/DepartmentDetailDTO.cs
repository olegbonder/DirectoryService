namespace DirectoryService.Contracts.Departments.GetDepartment
{
    public record DepartmentDetailDTO
    {
        public Guid Id { get; init; }

        public Guid? ParentId { get; init; }

        public string Name { get; init; } = null!;

        public string Identifier { get; init; } = null!;

        public string Path { get; init; } = null!;

        public int Depth { get; init; }

        public List<DictionaryItemResponse> Locations { get; init; } = null!;

        public List<string> Positions { get; init; } = null!;

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}

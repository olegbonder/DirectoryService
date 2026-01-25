namespace DirectoryService.Contracts.Positions.GetPositions
{
    public record PositionDTO
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;

        public string? Description { get; init; }

        public int DepartmentsCount { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}

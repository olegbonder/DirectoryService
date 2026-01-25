namespace DirectoryService.Contracts.Positions.GetPosition
{
    public record PositionDetailDTO
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;

        public string? Description { get; init; }

        public List<DictionaryItemResponse> Departments { get; init; } = null!;

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}

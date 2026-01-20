namespace DirectoryService.Contracts.Positions.GetPositions
{
    public record GetPositionsRequest : PaginationRequest
    {
        public List<Guid>? DepartmentIds { get; init; }
        public string? Search { get; init; }
        public bool? IsActive { get; init; }
    }
}

namespace DirectoryService.Contracts.Locations.GetLocations
{
    public record GetLocationsRequest : PaginationRequest
    {
        public List<Guid>? DepartmentIds { get; init; }
        public string? Search { get; init; }
        public bool? IsActive { get; init; }
        public OrderBy? Order { get; init; }
    }

    public enum OrderBy
    {
        Asc,
        Desc
    }
}

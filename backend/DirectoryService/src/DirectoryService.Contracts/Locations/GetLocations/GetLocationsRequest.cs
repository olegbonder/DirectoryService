namespace DirectoryService.Contracts.Locations.GetLocations
{
    public record GetLocationsRequest : PaginationRequest
    {
        public List<Guid>? DepartmentIds { get; init; }
        public string? Search { get; init; }
        public bool? IsActive { get; init; }
        public OrderDirection? Order { get; init; }
    }
}

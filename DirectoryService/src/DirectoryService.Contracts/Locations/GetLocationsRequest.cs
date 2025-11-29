namespace DirectoryService.Contracts.Locations
{
    public record GetLocationsRequest
    {
        public GetLocationsRequest()
        {
            Pagination = new PaginationRequest();
        }

        public List<Guid>? DepartmentIds { get; init; }
        public string? Search { get; init; }
        public bool? IsActive { get; init; }
        public PaginationRequest Pagination { get; init; }
    }
}

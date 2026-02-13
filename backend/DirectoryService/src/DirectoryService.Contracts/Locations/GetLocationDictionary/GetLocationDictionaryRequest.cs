namespace DirectoryService.Contracts.Locations.GetLocationDictionary
{
    public record GetLocationDictionaryRequest : PaginationRequest
    {
        public string? Search { get; init; }
        public List<Guid>? LocationIds { get; init; }
    }
}

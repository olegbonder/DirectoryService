namespace DirectoryService.Contracts.Locations
{
    public record GetLocationsResponse(List<LocationDTO> Locations, long TotalCount);
}

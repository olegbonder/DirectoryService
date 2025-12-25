namespace DirectoryService.Contracts.Locations.GetLocations
{
    public record GetLocationsResponse(List<LocationDTO> Locations, long TotalCount);
}

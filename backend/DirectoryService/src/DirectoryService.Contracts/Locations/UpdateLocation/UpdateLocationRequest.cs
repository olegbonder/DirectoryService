namespace DirectoryService.Contracts.Locations.CreateLocation
{
    public record UpdateLocationRequest(string Name, AddressDTO Address, string TimeZone);
}

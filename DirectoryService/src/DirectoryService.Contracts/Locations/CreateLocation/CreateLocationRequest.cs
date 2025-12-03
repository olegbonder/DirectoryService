namespace DirectoryService.Contracts.Locations.CreateLocation
{
    public record CreateLocationRequest(string Name, AddressDTO Address, string TimeZone);
}

namespace DirectoryService.Contracts.Locations
{
    public record CreateLocationRequest(string Name, AddressDTO Address, string TimeZone);
}

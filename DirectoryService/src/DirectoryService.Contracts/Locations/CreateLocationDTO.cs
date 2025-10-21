namespace DirectoryService.Contracts.Locations
{
    public record CreateLocationDTO(string Name, AddressDTO Address, string TimeZone);
}

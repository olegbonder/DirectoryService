namespace DirectoryService.Contracts.Locations.CreateLocation
{
    public record AddressDTO(
        string Country,
        string City,
        string Street,
        string House,
        string? FlatNumber);
}

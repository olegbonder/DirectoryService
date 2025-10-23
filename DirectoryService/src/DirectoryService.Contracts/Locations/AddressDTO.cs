namespace DirectoryService.Contracts.Locations
{
    public record AddressDTO(
        string Country,
        string City,
        string Street,
        string House,
        string? FlatNumber);
}

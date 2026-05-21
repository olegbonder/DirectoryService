namespace AuthService.Contracts.Dtos.Admin.GetUsers
{
    public record UserDTO(Guid Id, string FirstName, string LastName, string Email, bool IsActive);
}
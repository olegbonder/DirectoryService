namespace SharedAuth
{
    public record UserInfo(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        IEnumerable<string> Roles,
        IEnumerable<string> Permissions);
}
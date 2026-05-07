namespace AuthService.Infrastructure.Seed;

public class AdminOptions
{
    public const string SECTION_NAME = "Admin";

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}
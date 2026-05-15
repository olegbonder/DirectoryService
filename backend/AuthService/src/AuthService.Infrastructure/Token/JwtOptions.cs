namespace AuthService.Infrastructure.Token;

public class JwtOptions
{
    public const string SECTION_NAME = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string Secret { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    public int RefreshTokenExpirationDays { get; set; } = 7;
}
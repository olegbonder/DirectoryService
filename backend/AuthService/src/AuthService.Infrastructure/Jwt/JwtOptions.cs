namespace AuthService.Infrastructure.Jwt;

public class JwtOptions
{
    public const string SECTION_NAME = "Jwt";

    public string Issuer { get; init; }

    public string Audience { get; init; }

    public string Secret { get; init; }

    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    public int RefreshTokenExpirationDays { get; set; } = 7;
}
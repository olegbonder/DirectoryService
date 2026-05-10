namespace AuthService.Domain.Token;

public class RefreshToken
{
    // EF Core
    private RefreshToken()
    {
    }

    public Guid Id { get; init; }

    public Base64Token Token { get; init; }

    public Guid UserId { get; init; }

    public ApplicationUser User { get; init; }

    public DateTime ExpiresAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByToken { get; set; }

    public RefreshToken(
        Guid id,
        Guid userId,
        DateTime expiresAt)
    {
        Id = id;
        Token = Base64Token.Create();
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}
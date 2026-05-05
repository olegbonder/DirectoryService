namespace AuthService.Domain.Token;

public class RefreshToken
{
    // EF Core
    private RefreshToken()
    {
    }

    public Guid Id { get; init; }

    public required Base64Token Token { get; init; }

    public Guid UserId { get; init; }

    public ApplicationUser User { get; init; }

    public DateTime ExpiresAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? RevokedAt { get; init; }

    public string? ReplacedByToken { get; init; }

    public RefreshToken(
        Guid id,
        Base64Token token,
        Guid userId,
        DateTime expiresAt)
    {
        Id = id;
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }
}
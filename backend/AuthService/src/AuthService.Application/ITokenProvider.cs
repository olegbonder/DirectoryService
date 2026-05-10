using AuthService.Domain;
using AuthService.Domain.Token;
using SharedKernel.Result;

namespace AuthService.Application;

public interface ITokenProvider
{
    Result<string?> GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);

    Task<Result<RefreshToken>> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<RefreshToken>> RotateRefreshTokenAsync(string token, CancellationToken cancellationToken);

    Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result> CleanupExpiredTokensAsync(DateTime olderThan, CancellationToken cancellationToken);
}
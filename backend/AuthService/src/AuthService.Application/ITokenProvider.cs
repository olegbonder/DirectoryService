using AuthService.Application.Model;
using AuthService.Domain;
using AuthService.Domain.Token;
using SharedKernel.Result;

namespace AuthService.Application;

public interface ITokenProvider
{
    Result<AccessToken> GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);

    Result<Guid> ExtactUserIdFromAccessToken(string accessToken);

    Task<Result<RefreshToken>> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<RefreshToken>> RotateRefreshTokenAsync(Guid userId, string token, CancellationToken cancellationToken);

    Task<Result> RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken);
}
using System.Linq.Expressions;
using AuthService.Domain.Token;
using SharedKernel.Result;

namespace AuthService.Application;

public interface IRefreshTokenRepository
{
    Task<Result<Guid>> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task<RefreshToken?> GetBy(
        Expression<Func<RefreshToken, bool>> predicate,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RefreshToken>> GetCollectionBy(
        Expression<Func<RefreshToken, bool>> predicate,
        CancellationToken cancellationToken);

    void DeleteTokensAsync(IEnumerable<RefreshToken> refreshTokens);
}
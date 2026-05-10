using System.Linq.Expressions;
using AuthService.Application;
using AuthService.Domain.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace AuthService.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(AuthDbContext context, ILogger<RefreshTokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

            return refreshToken.Id;
        }
        catch(OperationCanceledException ex)
        {
            _logger.LogError(ex, "Cancel add operation for {RefreshToken}", refreshToken.Token);
            return GeneralErrors.OperationCancelled("create.refresh_token");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error add operation for {RefreshToken}", refreshToken.Token);
            return GeneralErrors.Failure(ex.Message);
        }
    }

    public async Task<RefreshToken?> GetBy(
        Expression<Func<RefreshToken, bool>> predicate, CancellationToken cancellationToken) =>
        await _context.RefreshTokens.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<IReadOnlyCollection<RefreshToken>> GetCollectionBy(
        Expression<Func<RefreshToken, bool>> predicate, CancellationToken cancellationToken) =>
        await _context.RefreshTokens.Where(predicate).ToListAsync(cancellationToken);

    public void DeleteTokensAsync(IEnumerable<RefreshToken> refreshTokens) =>
        _context.RefreshTokens.RemoveRange(refreshTokens);
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application;
using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Domain.Permissions;
using AuthService.Domain.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Result;

namespace AuthService.Infrastructure.Jwt;

public class TokenProvider : ITokenProvider
{
    private readonly JwtOptions _options;
    private readonly IRefreshTokenRepository _repository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<TokenProvider> _logger;

    public TokenProvider(
        IOptions<JwtOptions> options,
        IRefreshTokenRepository repository,
        ITransactionManager transactionManager,
        ILogger<TokenProvider> logger)
    {
        _options = options.Value;
        _repository = repository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public Result<string?> GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var roleList = roles.ToList();
        foreach (string role in roleList)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var permissions = RolePermissions.GetPermissions(roleList);

        foreach (string permission in permissions)
        {
            claims.Add(new Claim(ClaimTypes.Authentication, permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        try
        {
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes),
                signingCredentials: credentials);

            string? jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return Result<string?>.Success(jwtToken);
        }
        catch (Exception ex)
        {
            return Error.Failure("jwt.generate", ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<Result<RefreshToken>> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.RefreshTokenExpirationDays);
        var refreshToken = new RefreshToken(id, userId, expiresAt);

        var createResult = await _repository.CreateAsync(refreshToken, cancellationToken);
        if (createResult.IsFailure)
        {
            return createResult.Errors;
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Errors;
        }

        _logger.LogInformation(
            "Refresh token created for user {UserId}",
            userId);

        return refreshToken;
    }

    public async Task<Result<RefreshToken>> RotateRefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        var existingToken = await _repository
            .GetBy(rt => rt.Token.Value == token, cancellationToken);

        if (existingToken == null)
        {
            _logger.LogWarning("Refresh token not found: {Token}", token);
            return Error.NotFound("refresh_token.not.found", "Refresh token not found");
        }

        var userId = existingToken.UserId;

        if (existingToken.RevokedAt.HasValue)
        {
            _logger.LogWarning("Attempt to use revoked refresh token for user {UserId}", userId);
        }

        var newTokenResult = await CreateRefreshTokenAsync(userId, cancellationToken);
        if (newTokenResult.IsFailure)
        {
            return newTokenResult.Errors;
        }

        var newToken = newTokenResult.Value;
        existingToken.Revoke();
        existingToken.ReplacedByToken = newToken.Token.Value;

        _logger.LogInformation(
            "Refresh token rotated for user {UserId}. Old token: {OldToken}, New token: {NewToken}",
            existingToken.UserId, existingToken.Token.Value, newToken.Token.Value);

        return newToken;
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userTokens = await _repository
            .GetCollectionBy(rt => rt.UserId == userId && !rt.RevokedAt.HasValue, cancellationToken);

        foreach (var token in userTokens)
        {
            token.Revoke();
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "ALL refresh tokens revoked for user {UserId}. Tokens affected: {Count}",
            userId,
            userTokens.Count);
    }

    public async Task<Result> CleanupExpiredTokensAsync(DateTime olderThan, CancellationToken cancellationToken)
    {
        var expiredTokens = await _repository
            .GetCollectionBy(rt => rt.ExpiresAt < olderThan, cancellationToken);

        if (expiredTokens.Any())
        {
            _repository.DeleteTokensAsync(expiredTokens);
            var deleteResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (deleteResult.IsFailure)
            {
                return deleteResult.Errors;
            }

            _logger.LogInformation("Cleaned up expired refresh tokens older than {Date}", olderThan);
        }

        return Result.Success();
    }
}
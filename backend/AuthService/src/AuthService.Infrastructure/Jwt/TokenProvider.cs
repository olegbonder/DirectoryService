using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application;
using AuthService.Application.Database;
using AuthService.Application.Model;
using AuthService.Domain;
using AuthService.Domain.Permissions;
using AuthService.Domain.Shared;
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

    public Result<AccessToken> GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles)
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
        var currentDt = DateTime.UtcNow;
        var expires = currentDt.AddMinutes(_options.AccessTokenLifetimeMinutes);
        try
        {
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var accessToken = new AccessToken(jwtToken, (int)(expires - currentDt).TotalSeconds);

            return Result<AccessToken>.Success(accessToken);
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

    public async Task<Result<RefreshToken>> RotateRefreshTokenAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        var existingToken = await _repository
            .GetBy(rt => rt.UserId == userId && rt.Token.Value == token, cancellationToken);

        if (existingToken == null)
        {
            _logger.LogWarning("Refresh token not found: {Token}", token);
            return TokenErrors.RefreshTokenNotFound();
        }

        if (existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expired for user {UserId}", userId);
            return TokenErrors.RefreshTokenExpired();
        }

        if (existingToken.RevokedAt.HasValue)
        {
            _logger.LogWarning("Attempt to use revoked refresh token for user {UserId}", userId);

            var revokeResult = await RevokeAllUserRefreshTokensAsync(userId, cancellationToken);
            if (revokeResult.IsFailure)
            {
                return revokeResult.Errors;
            }
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

    public async Task<Result> RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userTokens = await _repository
            .GetCollectionBy(rt => rt.UserId == userId && !rt.RevokedAt.HasValue, cancellationToken);

        foreach (var token in userTokens)
        {
            token.Revoke();
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Errors;
        }

        _logger.LogWarning(
            "ALL refresh tokens revoked for user {UserId}. Tokens affected: {Count}",
            userId,
            userTokens.Count);

        return Result.Success();
    }

    public async Task<Result> CleanupExpiredTokensAsync(DateTime olderThan, CancellationToken cancellationToken)
    {
        var expiredTokens = await _repository
            .GetCollectionBy(rt => rt.ExpiresAt < olderThan, cancellationToken);

        if (expiredTokens.Any())
        {
            _repository.DeleteTokensAsync(expiredTokens);
            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                return saveResult.Errors;
            }

            _logger.LogInformation("Cleaned up expired refresh tokens older than {Date}", olderThan);
        }

        return Result.Success();
    }

    public Result<Guid> ExtactUserIdFromAccessToken(string accessToken)
    {
        var result = ValidateAccessToken(accessToken);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        var principal = result.Value;
        if (principal == null)
        {
            return TokenErrors.InvalidAccessToken();
        }

        var userIdClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Error.Failure("jwt.not.found.user_id.claim", "Not found user id claim in access token");
        }

        var tryParseUserId = Guid.TryParse(userIdClaim, out var userId);
        if (!tryParseUserId)
        {
            _logger.LogError("Incorrect guid for user id claim in access token {AccessToken}", accessToken);
            return TokenErrors.IncorrectGuidForUserIdInAccessToken();
        }

        return Result<Guid>.Success(userId);
    }

    private Result<ClaimsPrincipal> ValidateAccessToken(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.Secret);

        try
        {
            var principal = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            }, out SecurityToken validatedToken);

            return Result<ClaimsPrincipal>.Success(principal);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Invalid access token: {Message}", ex.Message);
            return TokenErrors.InvalidAccessToken();
        }
    }
}
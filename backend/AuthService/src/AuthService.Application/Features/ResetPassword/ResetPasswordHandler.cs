using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Result;

namespace AuthService.Application.Features.ResetPassword;

public class ResetPasswordHandler : IResultCommandHandler<ResetPasswordCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        UserManager<ApplicationUser> userManager,
        IRefreshTokenRepository refreshTokenRepo,
        ITokenProvider tokenProvider,
        ITransactionManager transactionManager,
        ILogger<ResetPasswordHandler> logger)
    {
        _userManager = userManager;
        _refreshTokenRepo = refreshTokenRepo;
        _tokenProvider = tokenProvider;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var userId = request.UserId;

        await _transactionManager.BeginTransactionAsync(cancellationToken);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return UserErrors.UserNotFound(userId);
        }

        var refreshTokenStr = Base64UrlEncoder.Decode(request.Token);
        var refreshToken = await _refreshTokenRepo.GetBy(rt => rt.UserId == userId && rt.Token.Value == refreshTokenStr, cancellationToken);
        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token not found for user {UserId}", userId);

            await _transactionManager.RollbackAsync(cancellationToken);
            return TokenErrors.RefreshTokenNotFound();
        }

        if (refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expired for user {UserId}", userId);

            await _transactionManager.RollbackAsync(cancellationToken);
            return TokenErrors.RefreshTokenExpired();
        }

        var resetPasswordResult = await _userManager.ResetPasswordAsync(user, refreshTokenStr, request.NewPassword);
        if (!resetPasswordResult.Succeeded)
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            return resetPasswordResult.Errors.ToErrors();
        }

        var revokeResult = await _tokenProvider.RevokeAllUserRefreshTokensAsync(userId, cancellationToken);
        if (revokeResult.IsFailure)
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            return revokeResult.Errors;
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            return saveResult.Errors;
        }

        await _transactionManager.CommitTransactionAsync(cancellationToken);

        _logger.LogInformation("User's {UserId} password being reset",  userId);

        return Result.Success();
    }
}
using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace AuthService.Application.Features.Logout;

public sealed class LogoutHandler : IResultCommandHandler<LogoutCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<LogoutCommand> _validator;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITransactionManager _transactionManager;
    private readonly IRefreshTokenCookieManager _cookieManager;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<LogoutCommand> validator,
        ITokenProvider tokenProvider,
        ITransactionManager transactionManager,
        IRefreshTokenCookieManager cookieManager,
        ILogger<LogoutHandler> logger)
    {
        _userManager = userManager;
        _validator = validator;
        _tokenProvider = tokenProvider;
        _transactionManager = transactionManager;
        _cookieManager = cookieManager;
        _logger = logger;
    }

    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        string accessToken = request.AccessToken;
        var userIdClaimResult = _tokenProvider.ExtactUserIdFromAccessToken(accessToken);
        if (userIdClaimResult.IsFailure)
        {
            _logger.LogError("User id claim not found in access token {AccessToken}", accessToken);
            return userIdClaimResult.Errors;
        }

        var userId = userIdClaimResult.Value;
        await _transactionManager.BeginTransactionAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            _logger.LogError("User {UserId} not found", userId);
            return UserErrors.UserNotFound(userId);
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

        _cookieManager.Delete();

        _logger.LogInformation("Logout user {UserId}", userId);

        return Result.Success();
    }
}
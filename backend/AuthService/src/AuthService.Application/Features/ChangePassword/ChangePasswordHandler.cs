using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SharedAuth.UserScope;
using SharedKernel.Result;

namespace AuthService.Application.Features.ChangePassword;

public class ChangePasswordHandler : IResultCommandHandler<ChangePasswordCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserScopedData _userScopedData;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<ChangePasswordCommand> _validator;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        UserManager<ApplicationUser> userManager,
        IUserScopedData userScopedData,
        ITokenProvider tokenProvider,
        ITransactionManager transactionManager,
        IValidator<ChangePasswordCommand> validator,
        ILogger<ChangePasswordHandler> logger)
    {
        _userManager = userManager;
        _userScopedData = userScopedData;
        _tokenProvider = tokenProvider;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        var currentPassword = request.CurrentPassword;

        var email = _userScopedData.Profile.Email;
        var userId = _userScopedData.Profile.Id;

        await _transactionManager.BeginTransactionAsync(cancellationToken);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogError("User {UserId} not found", userId);
            await _transactionManager.RollbackAsync(cancellationToken);
            return UserErrors.UserNotFound(userId);
        }

        bool checkCurrentPasswordResult = await _userManager.CheckPasswordAsync(user, currentPassword);
        if (!checkCurrentPasswordResult)
        {
            _logger.LogError("Failed password for user {UserId}", userId);
            await _transactionManager.RollbackAsync(cancellationToken);
            return UserErrors.FailedLoginOrPassword();
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, currentPassword, request.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            _logger.LogError("Failed change password for user {UserId}", userId);
            await _transactionManager.RollbackAsync(cancellationToken);
            return changePasswordResult.Errors.ToErrors();
        }

        var revokeResult = await _tokenProvider.RevokeAllUserRefreshTokensAsync(userId, cancellationToken);
        if (revokeResult.IsFailure)
        {
            _logger.LogError("Failed revoke refresh tokens for user {UserId}", userId);
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

        _logger.LogInformation("Change password success for user {UserId}",  userId);

        return Result.Success();
    }
}
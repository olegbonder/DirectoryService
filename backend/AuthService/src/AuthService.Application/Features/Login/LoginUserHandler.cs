using AuthService.Application.Database;
using AuthService.Contracts.Dtos.Login;
using AuthService.Domain;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace AuthService.Application.Features.Login;

public sealed class LoginUserHandler : ICommandHandler<LoginResponse, LoginUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<LoginUserCommand> _validator;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<LoginUserHandler> _logger;

    public LoginUserHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<LoginUserCommand> validator,
        ITokenProvider tokenProvider,
        ITransactionManager transactionManager,
        ILogger<LoginUserHandler> logger)
    {
        _userManager = userManager;
        _validator = validator;
        _tokenProvider = tokenProvider;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        string email = request.Email;

        await _transactionManager.BeginTransactionAsync(cancellationToken);
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogError("User {Email} not found", email);
            await _transactionManager.RollbackAsync(cancellationToken);
            return UserErrors.FailedLoginOrPassword();
        }

        if (!user.IsActive)
        {
            _logger.LogError("User {Email} is not active", email);
            await _transactionManager.RollbackAsync(cancellationToken);
            return UserErrors.InActiveUser();
        }

        bool isEmailConfirmedResult = await _userManager.IsEmailConfirmedAsync(user);
        if (!isEmailConfirmedResult)
        {
            _logger.LogError("User {Email} is not confirmed", email);
            await _transactionManager.RollbackAsync(cancellationToken);
            return UserErrors.UserEmailNotConfirmed();
        }

        bool isLockedOutResult = await _userManager.IsLockedOutAsync(user);
        if (isLockedOutResult)
        {
            var endDateLockOut = await _userManager.GetLockoutEndDateAsync(user);
            _logger.LogError("User {Email} is locked out {EndDate}", email, endDateLockOut);
            await _transactionManager.RollbackAsync(cancellationToken);
            return UserErrors.UserLockedOut(endDateLockOut);
        }

        bool checkPasswordResult = await _userManager.CheckPasswordAsync(user, request.Password);
        if (checkPasswordResult)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }
        else
        {
            _logger.LogError("Failed login or password for user {Email}", email);
            await _userManager.AccessFailedAsync(user);
            await _transactionManager.SaveChangesAsync(cancellationToken);
            await _transactionManager.CommitTransactionAsync(cancellationToken);
            return UserErrors.FailedLoginOrPassword();
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var accessTokenResult = _tokenProvider.GenerateAccessToken(user, userRoles);
        if (accessTokenResult.IsFailure)
        {
            _logger.LogError("Failed generate access token for user {Email}", email);
            await _transactionManager.RollbackAsync(cancellationToken);
            return accessTokenResult.Errors;
        }

        var refreshTokenResult = await _tokenProvider.CreateRefreshTokenAsync(user.Id, cancellationToken);
        if (refreshTokenResult.IsFailure)
        {
            _logger.LogError("Failed generate refresh token for user {Email}", email);
            await _transactionManager.RollbackAsync(cancellationToken);
            return refreshTokenResult.Errors;
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Errors;
        }

        await _transactionManager.CommitTransactionAsync(cancellationToken);

        _logger.LogInformation("User {Email} logged in", email);

        var result = new LoginResponse(
            accessTokenResult.Value.Token,
            refreshTokenResult.Value.Token.Value,
            accessTokenResult.Value.ExpiresAt);

        return result;
    }
}
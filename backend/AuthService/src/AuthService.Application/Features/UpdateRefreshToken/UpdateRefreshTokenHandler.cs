using AuthService.Application.Database;
using AuthService.Contracts.Dtos.Login;
using AuthService.Domain;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace AuthService.Application.Features.UpdateRefreshToken;

public sealed class UpdateRefreshTokenHandler : ICommandHandler<LoginResponse, UpdateRefreshTokenCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<UpdateRefreshTokenCommand> _validator;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateRefreshTokenHandler> _logger;

    public UpdateRefreshTokenHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<UpdateRefreshTokenCommand> validator,
        ITokenProvider tokenProvider,
        ITransactionManager transactionManager,
        ILogger<UpdateRefreshTokenHandler> logger)
    {
        _userManager = userManager;
        _validator = validator;
        _tokenProvider = tokenProvider;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(UpdateRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        string accessToken = request.AccessToken;
        string refreshToken = request.RefreshToken;
        var userIdClaimResult = _tokenProvider.ExtactUserIdFromAccessToken(accessToken);
        if (userIdClaimResult.IsFailure)
        {
            _logger.LogError("User id claim not found in access token {AccessToken}", accessToken);
            return userIdClaimResult.Errors;
        }

        var userIdStr = userIdClaimResult.Value;
        var tryParseUserId = Guid.TryParse(userIdStr, out var userId);
        if (!tryParseUserId)
        {
            _logger.LogError("Incorrect guid for user id claim in access token {AccessToken}", accessToken);
            return GeneralErrors.Failure("Incorrect guid for user id claim in access token");
        }

        await _transactionManager.BeginTransactionAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(userIdStr);
        if (user == null)
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            _logger.LogError("User {UserId} not found", userId);
            return GeneralErrors.NotFound("user.not.found", userId);
        }

        var refreshTokenResult = await _tokenProvider.RotateRefreshTokenAsync(userId, refreshToken, cancellationToken);
        if (refreshTokenResult.IsFailure)
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            return refreshTokenResult.Errors;
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var accessTokenResult = _tokenProvider.GenerateAccessToken(user, userRoles);
        if (accessTokenResult.IsFailure)
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            return accessTokenResult.Errors;
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            return saveResult.Errors;
        }

        await _transactionManager.CommitTransactionAsync(cancellationToken);

        _logger.LogInformation("Refresh token updated for user {UserId}", userId);

        var accessTokenDto = accessTokenResult.Value;
        var loginResponse = new LoginResponse(
            accessTokenDto.Token,
            refreshTokenResult.Value.Token.Value,
            accessTokenDto.ExpiresAt);
        return loginResponse;
    }
}
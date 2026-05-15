using AuthService.Contracts.Dtos.VerifyEmail;
using AuthService.Domain;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Result;

namespace AuthService.Application.Features.VerifyResetPassword;

public class VerifyResetPasswordHandler : IQueryHandler<VerifyEmailRequest, VerifyEmailRequest>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<VerifyResetPasswordHandler> _logger;

    public VerifyResetPasswordHandler(
        UserManager<ApplicationUser> userManager,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<VerifyResetPasswordHandler> logger)
    {
        _userManager = userManager;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<Result<VerifyEmailRequest>> Handle(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var token = request.Token;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return GeneralErrors.NotFound(nameof(userId), userId);
        }

        var refreshToken = await _refreshTokenRepository.GetBy(rt => rt.UserId == userId && rt.Token.Value == token, cancellationToken);
        if (refreshToken == null)
        {
            return TokenErrors.RefreshTokenNotFound();
        }

        _logger.LogInformation("Get User {UserId} and refresh token {Token} success", userId, token);

        return request;
    }
}
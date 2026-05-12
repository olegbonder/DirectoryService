using AuthService.Domain;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Result;

namespace AuthService.Application.Features.VerifyEmail;

public class VerifyEmailHandler : ICommandHandler<Guid, VerifyEmailCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<VerifyEmailHandler> _logger;

    public VerifyEmailHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<VerifyEmailHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var userId = request.UserId;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return GeneralErrors.NotFound(nameof(request.UserId), userId);
        }

        var confirmResult = await _userManager.ConfirmEmailAsync(user, Base64UrlEncoder.Decode(request.Token));
        if (!confirmResult.Succeeded)
        {
            return confirmResult.Errors.ToErrors();
        }

        _logger.LogInformation("User {UserId} is verified",  userId);

        return userId;
    }
}
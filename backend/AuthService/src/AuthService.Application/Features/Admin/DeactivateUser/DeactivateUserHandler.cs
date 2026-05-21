using AuthService.Domain;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace AuthService.Application.Features.Admin.DeactivateUser;

public class DeactivateUserHandler : IResultCommandHandler<DeactivateUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<DeactivateUserCommand> _validator;
    private readonly ILogger<DeactivateUserHandler> _logger;

    public DeactivateUserHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<DeactivateUserCommand> validator,
        ILogger<DeactivateUserHandler> logger)
    {
        _userManager = userManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var userId = command.UserId;

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogError("User {UserId} not found", userId);
            return UserErrors.UserNotFound(userId);
        }

        user.Deactivate();
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError("Failed to deactivate user {UserId}", userId);
            return updateResult.Errors.ToErrors();
        }

        _logger.LogInformation("User {UserId} deactivated successfully", userId);

        return Result.Success();
    }
}
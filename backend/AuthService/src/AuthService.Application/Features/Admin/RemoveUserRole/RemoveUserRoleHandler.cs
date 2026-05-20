using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace AuthService.Application.Features.Admin.RemoveUserRole;

public class RemoveUserRoleHandler : IResultCommandHandler<RemoveUserRoleCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IValidator<RemoveUserRoleCommand> _validator;
    private readonly ILogger<RemoveUserRoleHandler> _logger;

    public RemoveUserRoleHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IValidator<RemoveUserRoleCommand> validator,
        ILogger<RemoveUserRoleHandler> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveUserRoleCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var role = command.Role;
        var userId = command.UserId;

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogError("User {UserId} not found", userId);
            return UserErrors.UserNotFound(userId);
        }

        var existingRole = await _roleManager.FindByNameAsync(role);
        if (existingRole == null)
        {
            _logger.LogError("Role {Role} not found", role);
            return UserErrors.RoleNotFound(role);
        }

        var addToRoleResult = await _userManager.RemoveFromRoleAsync(user, role);
        if (!addToRoleResult.Succeeded)
        {
            _logger.LogError("Failed to remove role {Role} from user {UserId}", role, userId);
            return addToRoleResult.Errors.ToErrors();
        }

        _logger.LogInformation("Role {Role} removed successfully for user {UserId}", role, userId);

        return Result.Success();
    }
}
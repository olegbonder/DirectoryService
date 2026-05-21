using AuthService.Domain.Shared;
using Core.Validation;
using FluentValidation;

namespace AuthService.Application.Features.Admin.RemoveUserRole;

public class RemoveUserRoleValidator : AbstractValidator<RemoveUserRoleCommand>
{
    public RemoveUserRoleValidator()
    {
        RuleFor(f => f.UserId)
            .NotNull()
            .NotEmpty()
            .WithError(UserErrors.UserIdIsNull());
        RuleFor(f => f.Role)
            .NotNull()
            .NotEmpty()
            .WithError(UserErrors.RoleIsEmpty());
    }
}
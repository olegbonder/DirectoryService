using AuthService.Domain.Shared;
using Core.Validation;
using FluentValidation;
using SharedKernel.Result;

namespace AuthService.Application.Features.Admin.AddUserRole;

public class AddUserRoleValidator : AbstractValidator<AddUserRoleCommand>
{
    public AddUserRoleValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.Role)
            .NotNull()
            .NotEmpty()
            .WithError(UserErrors.RoleIsEmpty());
    }
}
using AuthService.Domain.Shared;
using Core.Validation;
using FluentValidation;
using SharedKernel.Result;

namespace AuthService.Application.Features.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.CurrentPassword)
            .NotNull()
            .NotEmpty()
            .WithError(UserErrors.PasswordIsEmpty());
        RuleFor(f => f.Request.NewPassword)
            .NotNull()
            .NotEmpty()
            .WithError(UserErrors.PasswordIsEmpty());
    }
}
using AuthService.Domain.Shared;
using Core.Validation;
using FluentValidation;

namespace AuthService.Application.Features.Admin.DeactivateUser;

public class DeactivateUserValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserValidator()
    {
        RuleFor(f => f.UserId)
            .NotNull()
            .NotEmpty()
            .WithError(UserErrors.UserIdIsNull());
    }
}
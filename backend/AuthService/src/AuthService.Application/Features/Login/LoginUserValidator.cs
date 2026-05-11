using Core.Validation;
using FluentValidation;
using SharedKernel.Result;

namespace AuthService.Application.Features.Login;

public class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.Email)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("user.email"))
            .EmailAddress()
            .WithError(Error.Validation("user.email", "Not valid email"));
        RuleFor(f => f.Request.Password)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("user.password"))
            .MinimumLength(8)
            .WithError(Error.Validation(
                "user.password.min_length",
                "Password must contain at least 8 characters"))
            .Matches(@"[A-Z]+")
            .WithError(Error.Validation(
                "user.password.upper_case",
                "Password must contain at least one upper case letter"))
            .Matches(@"[0-9]+")
            .WithError(Error.Validation(
                "user.password.digit",
                "Password must contain at least one digit"))
            .Matches(@"[\!\?\*\.@#$%^&+=]+")
            .WithError(Error.Validation(
                "user.password.special_character",
                "Password must contain at least one special character (e.g., ! ? * . @ # $ % ^ & + =)."));
    }
}
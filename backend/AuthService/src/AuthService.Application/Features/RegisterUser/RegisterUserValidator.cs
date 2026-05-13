using AuthService.Domain.Shared;
using Core.Validation;
using FluentValidation;
using SharedKernel.Result;

namespace AuthService.Application.Features.RegisterUser;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.Email)
            .NotEmpty()
            .WithError(UserErrors.EmailIsEmpty())
            .EmailAddress()
            .WithError(UserErrors.EmailNotValid());
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
        RuleFor(f => f.Request.FirstName)
            .NotNull()
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("user.first_name"));
        RuleFor(f => f.Request.LastName)
            .NotNull()
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("user.last_name"));
    }
}
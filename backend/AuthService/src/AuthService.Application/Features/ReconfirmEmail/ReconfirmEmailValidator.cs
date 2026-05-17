using AuthService.Domain.Shared;
using Core.Validation;
using FluentValidation;
using SharedKernel.Result;

namespace AuthService.Application.Features.ReconfirmEmail;

public class ReconfirmEmailValidator : AbstractValidator<ReconfirmEmailCommand>
{
    public ReconfirmEmailValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.Email)
            .NotEmpty()
            .WithError(UserErrors.EmailIsEmpty())
            .EmailAddress()
            .WithError(UserErrors.EmailNotValid());
    }
}
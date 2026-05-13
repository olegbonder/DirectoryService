using Core.Validation;
using FluentValidation;
using SharedKernel.Result;

namespace AuthService.Application.Features.Logout;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.AccessToken)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("user.access_token"));
    }
}
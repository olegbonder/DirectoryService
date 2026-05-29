using Core.Validation;
using FluentValidation;
using SharedKernel.Result;

namespace AuthService.Application.Features.UpdateRefreshToken;

public class UpdateRefreshTokenValidator : AbstractValidator<UpdateRefreshTokenCommand>
{
    public UpdateRefreshTokenValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.AccessToken)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("user.access_token"));
    }
}
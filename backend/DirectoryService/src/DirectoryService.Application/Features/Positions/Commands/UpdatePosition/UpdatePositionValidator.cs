using Core.Validation;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Positions.Commands.UpdatePosition
{
    public class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
    {
        public UpdatePositionValidator()
        {
            RuleFor(l => l.PositionId)
                .NotNull()
                .WithError(PositionErrors.PositionIdNotBeNull())
                .NotEmpty()
                .WithError(PositionErrors.PositionIdNotBeEmpty());
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.RequestIsNull());
            RuleFor(l => l.Request.Name).MustBeValueObject(PositionName.Create);
            RuleFor(l => l.Request.Description).MustBeValueObject(PositionDesription.Create);
        }
    }
}

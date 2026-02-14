using Core.Validation;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Positions.Commands.CreatePosition
{
    public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
    {
        public CreatePositionValidator()
        {
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.RequestIsNull());
            RuleFor(l => l.Request.Name).MustBeValueObject(PositionName.Create);
            RuleFor(l => l.Request.Description).MustBeValueObject(PositionDesription.Create);
            RuleFor(l => l.Request.DepartmentIds)
                .NotNull()
                .WithError(PositionErrors.DepartmentIdsNotBeNull())
                .NotEmpty()
                .WithError(PositionErrors.DepartmentIdsNotBeEmpty())
                .Must(l => l != null && l.Distinct().Count() == l.Count())
                .WithError(PositionErrors.DepartmentIdsMustBeUnique());
        }
    }
}

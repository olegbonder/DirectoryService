using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Shared.Result;

namespace DirectoryService.Application.Features.Positions.CreatePosition
{
    public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
    {
        public CreatePositionValidator()
        {
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));
            RuleFor(l => l.Request.Name).MustBeValueObject(PositionName.Create);
            RuleFor(l => l.Request.Description).MustBeValueObject(PositionDesription.Create);
            RuleFor(l => l.Request.DepartmentIds)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("Position.departmentIds"))
                .NotEmpty()
                .WithError(Error.Validation("Position.departmentIds.not.empty", "Список подразделений не может быть пустым"))
                .Must(l => l != null && l.Distinct().Count() == l.Count())
                .WithError(Error.Validation("Position.departmentIds.must.be.unique", "Список подразделений должен быть уникальным"));
        }
    }
}

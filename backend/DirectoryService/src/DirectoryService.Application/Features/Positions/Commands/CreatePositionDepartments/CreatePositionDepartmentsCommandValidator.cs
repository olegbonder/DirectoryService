using Core.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Positions.Commands.CreatePositionDepartments
{
    public class CreatePositionDepartmentsCommandValidator : AbstractValidator<CreatePositionDepartmentsCommand>
    {
        public CreatePositionDepartmentsCommandValidator()
        {
            RuleFor(p => p.PositionId)
                .NotNull()
                .WithError(PositionErrors.PositionIdNotBeNull())
                .NotEmpty()
                .WithError(PositionErrors.PositionIdNotBeEmpty());
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.RequestIsNull());
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

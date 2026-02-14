using Core.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Features.Positions.Commands.DeletePositionDepartment
{
    public class DeletePositionDepartmentValidator : AbstractValidator<DeletePositionDepartmentCommand>
    {
        public DeletePositionDepartmentValidator()
        {
            RuleFor(p => p.PositionId)
                .NotNull()
                .WithError(PositionErrors.PositionIdNotBeNull())
                .NotEmpty()
                .WithError(PositionErrors.PositionIdNotBeEmpty());
            RuleFor(l => l.DepartmentId)
                .NotNull()
                .WithError(DepartmentErrors.DepartmentIdNotBeNull())
                .NotEmpty()
                .WithError(DepartmentErrors.DepartmentIdNotBeEmpty());
        }
    }
}

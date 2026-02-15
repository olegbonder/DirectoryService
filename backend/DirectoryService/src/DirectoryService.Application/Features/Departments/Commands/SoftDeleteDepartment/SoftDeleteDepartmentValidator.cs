using Core.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Features.Departments.Commands.SoftDeleteDepartment
{
    public class SoftDeleteDepartmentValidator : AbstractValidator<SoftDeleteDepartmentCommand>
    {
        public SoftDeleteDepartmentValidator()
        {
            RuleFor(l => l.DepartmentId)
                .NotNull()
                .WithError(DepartmentErrors.DepartmentIdNotBeNull())
                .NotEmpty()
                .WithError(DepartmentErrors.DepartmentIdNotBeEmpty());
        }
    }
}

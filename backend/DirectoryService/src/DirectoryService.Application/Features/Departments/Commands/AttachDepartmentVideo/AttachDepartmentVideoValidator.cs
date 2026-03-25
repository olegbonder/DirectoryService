using Core.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Commands.AttachDepartmentVideo
{
    public sealed class AttachDepartmentVideoValidator : AbstractValidator<AttachDepartmentVideoCommand>
    {
        public AttachDepartmentVideoValidator()
        {
            RuleFor(l => l.DepartmentId)
                .NotNull()
                .WithError(DepartmentErrors.DepartmentIdNotBeNull())
                .NotEmpty()
                .WithError(DepartmentErrors.DepartmentIdNotBeEmpty());
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.RequestIsNull());
        }
    }
}

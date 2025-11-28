using DirectoryService.Application.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Commands.MoveDepartment
{
    public sealed class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
    {
        public MoveDepartmentValidator()
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

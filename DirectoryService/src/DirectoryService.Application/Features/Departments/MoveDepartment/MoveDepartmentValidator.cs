using DirectoryService.Application.Validation;
using FluentValidation;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.MoveDepartment
{
    public sealed class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
    {
        public MoveDepartmentValidator()
        {
            RuleFor(l => l.DepartmentId)
                .NotNull()
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("departmentId"));
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));
        }
    }
}

using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.CreateDepartment
{
    public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentValidator()
        {
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));
            RuleFor(l => l.Request.Name).MustBeValueObject(DepartmentName.Create);
            RuleFor(l => l.Request.Identifier).MustBeValueObject(DepartmentIdentifier.Create);
            RuleFor(l => l.Request.LocationIds)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("department.locationIds"))
                .NotEmpty()
                .WithError(Error.Validation("department.locationIds.not.empty", "Список локаций не может быть пустым"))
                .Must(l => l != null && l.Distinct().Count() == l.Count())
                .WithError(Error.Validation("department.locationIds.must.be.unique", "Список локаций должен быть уникальным"));
        }
    }
}

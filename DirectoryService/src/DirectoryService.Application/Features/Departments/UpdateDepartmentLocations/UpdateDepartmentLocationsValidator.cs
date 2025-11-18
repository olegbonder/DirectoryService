using DirectoryService.Application.Validation;
using FluentValidation;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.UpdateDepartmentLocations
{
    public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateLocationsCommand>
    {
        public UpdateDepartmentLocationsValidator()
        {
            RuleFor(l => l.DepartmentId)
                .NotNull()
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("departmentId"));
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));
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

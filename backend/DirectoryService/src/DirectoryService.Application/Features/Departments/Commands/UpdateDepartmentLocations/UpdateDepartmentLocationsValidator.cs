using Core.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartmentLocations
{
    public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateLocationsCommand>
    {
        public UpdateDepartmentLocationsValidator()
        {
            RuleFor(l => l.DepartmentId)
                .NotNull()
                .WithError(DepartmentErrors.DepartmentIdNotBeNull())
                .NotEmpty()
                .WithError(DepartmentErrors.DepartmentIdNotBeEmpty());
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.RequestIsNull());
            RuleFor(l => l.Request.LocationIds)
                .NotNull()
                .WithError(DepartmentErrors.LocationIdsNotBeNull())
                .NotEmpty()
                .WithError(DepartmentErrors.LocationIdsNotBeEmpty())
                .Must(l => l != null && l.Distinct().Count() == l.Count())
                .WithError(DepartmentErrors.LocationIdsMustBeUnique());
        }
    }
}

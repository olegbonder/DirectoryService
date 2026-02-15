using Core.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentValidator()
        {
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.RequestIsNull());
            RuleFor(l => l.Request.Name).MustBeValueObject(DepartmentName.Create);
            RuleFor(l => l.Request.Identifier).MustBeValueObject(DepartmentIdentifier.Create);
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

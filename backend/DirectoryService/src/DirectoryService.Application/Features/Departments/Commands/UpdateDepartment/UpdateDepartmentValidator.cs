using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartment
{
    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentValidator()
        {
            RuleFor(d => d.Id)
                .NotNull()
                .WithError(DepartmentErrors.DepartmentIdNotBeNull())
                .NotEmpty()
                .WithError(DepartmentErrors.DepartmentIdNotBeEmpty());
            RuleFor(d => d.Request)
                .NotNull()
                .WithError(GeneralErrors.RequestIsNull());
            RuleFor(d => d.Request.Name).MustBeValueObject(DepartmentName.Create);
            RuleFor(d => d.Request.Identifier).MustBeValueObject(DepartmentIdentifier.Create);
        }
    }
}

using Core.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Features.Locations.Commands.SoftDeleteLocation
{
    public class SoftDeleteLocationValidator : AbstractValidator<SoftDeleteLocationCommand>
    {
        public SoftDeleteLocationValidator()
        {
            RuleFor(l => l.LocationId)
                .NotNull()
                .WithError(LocationErrors.LocationIdNotBeNull())
                .NotEmpty()
                .WithError(LocationErrors.LocationIdNotBeEmpty());
        }
    }
}

using DirectoryService.Application.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Features.Positions.Commands.SoftDeletePosition
{
    public class SoftDeletePositionValidator : AbstractValidator<SoftDeletePositionCommand>
    {
        public SoftDeletePositionValidator()
        {
            RuleFor(l => l.PositionId)
                .NotNull()
                .WithError(LocationErrors.LocationIdNotBeNull())
                .NotEmpty()
                .WithError(LocationErrors.LocationIdNotBeEmpty());
        }
    }
}

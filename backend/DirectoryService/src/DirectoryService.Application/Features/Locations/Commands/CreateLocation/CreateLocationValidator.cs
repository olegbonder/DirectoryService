using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations.Commands.CreateLocation
{
    public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
    {
        public CreateLocationValidator()
        {
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));
            RuleFor(l => l.Request.Name).MustBeValueObject(LocationName.Create);
            RuleFor(l => l.Request.Address)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("location.address"));

            RuleFor(l => l.Request.Address)
                .MustBeValueObject(l =>
                    LocationAddress.Create(l.Country, l.City, l.Street, l.House, l.FlatNumber));

            RuleFor(l => l.Request.TimeZone).MustBeValueObject(LocationTimezone.Create);
        }
    }
}

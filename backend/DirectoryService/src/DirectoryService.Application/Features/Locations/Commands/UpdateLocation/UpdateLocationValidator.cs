using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared.Result;

namespace DirectoryService.Application.Features.Locations.Commands.UpdateLocation
{
    public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
    {
        public UpdateLocationValidator()
        {
            RuleFor(l => l.LocationId)
                .NotNull()
                .WithError(LocationErrors.LocationIdNotBeNull())
                .NotEmpty()
                .WithError(LocationErrors.LocationIdNotBeEmpty());
            RuleFor(l => l.Request)
                .NotNull()
                .WithError(GeneralErrors.RequestIsNull());
            RuleFor(l => l.Request.Name).MustBeValueObject(LocationName.Create);
            RuleFor(l => l.Request.Address)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("location.address"));

            RuleFor(l => l.Request.Address)
                .MustBeValueObject(l =>
                    LocationAddress.Create(l.Country, l.City, l.Street, l.House, l.Flat));

            RuleFor(l => l.Request.TimeZone).MustBeValueObject(LocationTimezone.Create);
        }
    }
}

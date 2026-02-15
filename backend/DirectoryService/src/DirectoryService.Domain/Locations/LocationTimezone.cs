using DirectoryService.Domain.Shared;
using SharedKernel.Result;

namespace DirectoryService.Domain.Locations
{
    public sealed record LocationTimezone
    {
        public LocationTimezone(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<LocationTimezone> Create(string timeZone)
        {
            if (string.IsNullOrWhiteSpace(timeZone))
            {
                return LocationErrors.TimezoneIsEmpty();
            }

            if(TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out TimeZoneInfo? tz) == false)
            {
                return LocationErrors.TimezoneInvalid();
            }

            return new LocationTimezone(timeZone);
        }
    }
}

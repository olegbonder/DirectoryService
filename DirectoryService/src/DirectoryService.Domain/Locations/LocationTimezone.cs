using Shared.Result;

namespace DirectoryService.Domain.Locations
{
    public class LocationTimezone
    {
        public LocationTimezone(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<LocationTimezone> Create(string timeZone)
        {
            string property = "location.timezone";

            if (string.IsNullOrWhiteSpace(timeZone))
            {
                return GeneralErrors.PropertyIsEmpty(property, "Часовой пояс");
            }

            if(TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out TimeZoneInfo? tz) == false)
            {
                return Error.Validation(property, "Указанное значение не является часовым поясом!");
            }

            return new LocationTimezone(timeZone);
        }
    }
}

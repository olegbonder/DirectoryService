using CSharpFunctionalExtensions;

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
            if (string.IsNullOrWhiteSpace(timeZone))
            {
                return Result.Failure<LocationTimezone>("Свойство \"Timezone\" не должно быть пустым");
            }

            if(TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out TimeZoneInfo? tz) == false)
            {
                return Result.Failure<LocationTimezone>("Указанное значение не является часовым поясом!");
            }

            return new LocationTimezone(timeZone);
        }
    }
}

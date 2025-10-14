namespace DirectoryService.Domain.Location
{
    public class LocationTimezone
    {
        public LocationTimezone(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Свойство \"Timezone\" не должно быть пустым");
            }

            var tz = TimeZoneInfo.FindSystemTimeZoneById(value);
            Value = tz.Id;
        }

        public string Value { get; }
    }
}

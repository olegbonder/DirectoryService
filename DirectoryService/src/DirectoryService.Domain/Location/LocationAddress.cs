namespace DirectoryService.Domain.Location
{
    public class LocationAddress
    {
        public LocationAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Свойство \"Address\" не должно быть пустым");
            }

            Value = value;
        }

        public string Value { get; }
    }
}

namespace DirectoryService.Domain.Location
{
    public class PositionName
    {
        public const int MIN_LENGTH = 3;
        public const int MAX_LENGTH = 100;

        public PositionName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Свойство \"Name\" не должно быть пустым");
            }

            if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            {
                throw new ArgumentException($"Свойство \"Name\" не должно быть меньше {MIN_LENGTH} или больше {MAX_LENGTH} символов");
            }

            Value = value;
        }

        public string Value { get; }
    }
}

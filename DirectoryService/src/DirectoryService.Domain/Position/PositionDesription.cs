namespace DirectoryService.Domain.Position
{
    public class PositionDesription
    {
        private const int MAX_LENGTH = 1000;

        public PositionDesription(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) == false && value.Length > MAX_LENGTH)
            {
                throw new ArgumentException($"Свойство \"Description\" не должно быть быть больше {MAX_LENGTH} символов");
            }

            Value = value;
        }

        public string? Value { get; }
    }
}

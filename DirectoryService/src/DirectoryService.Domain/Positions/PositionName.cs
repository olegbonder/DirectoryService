using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions
{
    public class PositionName
    {
        private PositionName(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<PositionName> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Свойство \"Name\" не должно быть пустым";
            }

            if (name.Length < LengthConstants.LENGTH_3 || name.Length > LengthConstants.LENGTH_100)
            {
                return $"Свойство \"Name\" не должно быть меньше {LengthConstants.LENGTH_3} или больше {LengthConstants.LENGTH_100} символов";
            }

            return new PositionName(name);
        }
    }
}

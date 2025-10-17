using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations
{
    public class LocationName
    {
        private LocationName(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<LocationName> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<LocationName>("Свойство \"Name\" не должно быть пустым");
            }

            if (name.Length < LengthConstants.LENGTH_3 || name.Length > LengthConstants.LENGTH_120)
            {
                return Result.Failure<LocationName>($"Свойство \"Name\" не должно быть меньше {LengthConstants.LENGTH_3} или больше {LengthConstants.LENGTH_150} символов");
            }

            return new LocationName(name);
        }
    }
}

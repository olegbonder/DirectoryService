using Shared.Result;

namespace DirectoryService.Domain.Locations
{
    public sealed class LocationName
    {
        private LocationName(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<LocationName> Create(string name)
        {
            string property = "location.name";

            if (string.IsNullOrWhiteSpace(name))
            {
                return GeneralErrors.PropertyIsEmpty(property);
            }

            var min = LengthConstants.LENGTH_3;
            var max = LengthConstants.LENGTH_120;
            if (name.Length < min || name.Length > max)
            {
                return GeneralErrors.PropertyOutOfRange(property, min, max);
            }

            return new LocationName(name);
        }
    }
}

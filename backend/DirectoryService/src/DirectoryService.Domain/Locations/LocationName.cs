using DirectoryService.Domain.Shared;
using SharedKernel.Result;

namespace DirectoryService.Domain.Locations
{
    public sealed record LocationName
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
                return LocationErrors.NameIsEmpty();
            }

            var min = LengthConstants.LENGTH_3;
            var max = LengthConstants.LENGTH_120;
            if (name.Length < min || name.Length > max)
            {
                return LocationErrors.NameLengthOutOfRange(min, max);
            }

            return new LocationName(name);
        }
    }
}

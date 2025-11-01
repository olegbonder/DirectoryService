using Shared.Result;

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
            string property = "position.name";
            if (string.IsNullOrWhiteSpace(name))
            {
                return GeneralErrors.PropertyIsEmpty(property);
            }

            var min = LengthConstants.LENGTH_3;
            var max = LengthConstants.LENGTH_100;
            if (name.Length < min || name.Length > max)
            {
                return GeneralErrors.PropertyOutOfRange(property, min, max);
            }

            return new PositionName(name);
        }
    }
}

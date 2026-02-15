using DirectoryService.Domain.Shared;
using SharedKernel.Result;

namespace DirectoryService.Domain.Positions
{
    public sealed record PositionName
    {
        public PositionName(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<PositionName> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return PositionErrors.NameIsEmpty();
            }

            int min = LengthConstants.LENGTH_3;
            int max = LengthConstants.LENGTH_100;
            if (name.Length < min || name.Length > max)
            {
                return PositionErrors.NameLengthOutOfRange(min, max);
            }

            return new PositionName(name);
        }
    }
}

using DirectoryService.Domain.Shared;
using Shared.Result;

namespace DirectoryService.Domain.Positions
{
    public sealed record PositionDesription
    {
        private PositionDesription(string? value)
        {
            Value = value;
        }

        public string? Value { get; }

        public static Result<PositionDesription> Create(string? desription)
        {
            var max = LengthConstants.LENGTH_1000;
            if (string.IsNullOrWhiteSpace(desription) == false && desription.Length > max)
            {
                return PositionErrors.DescriptionLengthOutOfRange(max);
            }

            return new PositionDesription(desription);
        }
    }
}

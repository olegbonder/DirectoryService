using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions
{
    public class PositionDesription
    {
        private PositionDesription(string? value)
        {
            Value = value;
        }

        public string? Value { get; }

        public static Result<PositionDesription> Create(string? desription)
        {
            if (string.IsNullOrWhiteSpace(desription) == false && desription.Length > LengthConstants.LENGTH_1000)
            {
                throw new ArgumentException($"Свойство \"Description\" не должно быть быть больше {LengthConstants.LENGTH_1000} символов");
            }

            return new PositionDesription(desription);
        }
    }
}

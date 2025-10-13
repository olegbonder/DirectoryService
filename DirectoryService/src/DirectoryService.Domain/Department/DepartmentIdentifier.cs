using System.Text.RegularExpressions;

namespace DirectoryService.Domain.Department
{
    public class DepartmentIdentifier
    {
        public const int MIN_LENGTH = 3;
        public const int MAX_LENGTH = 150;
        public const string ONLY_LATIN_REGEX = @"^[a-zA-Z]+$";
        public DepartmentIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Свойство \"Identifier\" не должно быть пустым");
            }

            if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            {
                throw new ArgumentException($"Свойство \"Identifier\" не должно быть меньше {MIN_LENGTH} или больше {MAX_LENGTH} символов");
            }

            if (Regex.IsMatch(value, ONLY_LATIN_REGEX) == false)
            {
                throw new ArgumentException("Свойство \"Identifier\" должно содержать латинские буквы");
            }

            Value = value;
        }

        public string Value { get; }
    }
}

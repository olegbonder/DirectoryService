using DirectoryService.Domain.Shared;
using System.Text.RegularExpressions;

namespace DirectoryService.Domain.Departments
{
    public class DepartmentIdentifier
    {
        public const string ONLY_LATIN_REGEX = @"^[a-z]+$";

        private DepartmentIdentifier(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<DepartmentIdentifier> Create(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return "Свойство \"Identifier\" не должно быть пустым";
            }

            if (identifier.Length < LengthConstants.LENGTH_3 || identifier.Length > LengthConstants.LENGTH_150)
            {
                return $"Свойство \"Identifier\" не должно быть меньше {LengthConstants.LENGTH_3} или больше {LengthConstants.LENGTH_150} символов";
            }

            if (Regex.IsMatch(identifier, ONLY_LATIN_REGEX) == false)
            {
                return "Свойство \"Identifier\" должно содержать латинские буквы";
            }

            return new DepartmentIdentifier(identifier);
        }
    }
}

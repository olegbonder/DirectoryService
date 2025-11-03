using System.Text.RegularExpressions;
using Shared.Result;

namespace DirectoryService.Domain.Departments
{
    public sealed class DepartmentIdentifier
    {
        public const string ONLY_LATIN_REGEX = @"^[a-z]+$";

        private DepartmentIdentifier(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<DepartmentIdentifier> Create(string identifier)
        {
            string property = "department.identifier";
            string name = "Идентификатор";
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return GeneralErrors.PropertyIsEmpty(property, name);
            }

            var min = LengthConstants.LENGTH_3;
            var max = LengthConstants.LENGTH_150;
            if (identifier.Length < min || identifier.Length > max)
            {
                return GeneralErrors.PropertyOutOfRange(property, min, max, name);
            }

            if (Regex.IsMatch(identifier, ONLY_LATIN_REGEX) == false)
            {
                return Error.Validation(
                    "department.identifier.length.must.be.latin",
                    "Свойство \"Identifier\" должно содержать латинские буквы");
            }

            return new DepartmentIdentifier(identifier);
        }
    }
}

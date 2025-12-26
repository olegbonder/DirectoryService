using System.Text.RegularExpressions;
using DirectoryService.Domain.Shared;
using Shared.Result;

namespace DirectoryService.Domain.Departments
{
    public sealed record DepartmentIdentifier
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
                return DepartmentErrors.IdentifierIsEmpty();
            }

            var min = LengthConstants.LENGTH_3;
            var max = LengthConstants.LENGTH_150;
            if (identifier.Length < min || identifier.Length > max)
            {
                return DepartmentErrors.IdentifierLengthOutOfRange(min, max);
            }

            if (Regex.IsMatch(identifier, ONLY_LATIN_REGEX) == false)
            {
                return DepartmentErrors.IdentifierMustBeLatin();
            }

            return new DepartmentIdentifier(identifier);
        }
    }
}

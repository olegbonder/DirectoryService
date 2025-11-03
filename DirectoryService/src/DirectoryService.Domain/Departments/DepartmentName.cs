using Shared.Result;

namespace DirectoryService.Domain.Departments
{
    public sealed record DepartmentName
    {
        private DepartmentName(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<DepartmentName> Create(string name)
        {
            string property = "department.name";
            if (string.IsNullOrWhiteSpace(name))
            {
                return GeneralErrors.PropertyIsEmpty(property);
            }

            var min = LengthConstants.LENGTH_3;
            var max = LengthConstants.LENGTH_150;
            if (name.Length < min || name.Length > max)
            {
                return GeneralErrors.PropertyOutOfRange(property, min, max);
            }

            return new DepartmentName(name);
        }
    }
}

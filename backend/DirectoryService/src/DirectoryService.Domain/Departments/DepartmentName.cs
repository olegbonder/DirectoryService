using DirectoryService.Domain.Shared;
using SharedKernel.Result;

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
            if (string.IsNullOrWhiteSpace(name))
            {
                return DepartmentErrors.NameIsEmpty();
            }

            int min = LengthConstants.LENGTH_3;
            int max = LengthConstants.LENGTH_150;
            if (name.Length < min || name.Length > max)
            {
                return DepartmentErrors.NameLengthOutOfRange(min, max);
            }

            return new DepartmentName(name);
        }
    }
}

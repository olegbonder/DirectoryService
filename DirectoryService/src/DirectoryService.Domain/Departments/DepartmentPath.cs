using Shared.Result;

namespace DirectoryService.Domain.Departments
{
    public sealed record DepartmentPath
    {
        private const char SEPARATOR = '.';

        private DepartmentPath(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<DepartmentPath> Create(DepartmentIdentifier identifier, Department? parentDept = null)
        {
            var value = identifier.Value;
            value = parentDept == null ? value : $"{parentDept.Path.Value}{SEPARATOR}{value}";
            return new DepartmentPath(value);
        }
    }
}

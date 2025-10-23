using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments
{
    public class DepartmentPath
    {
        private const char SEPARATOR = '.';

        private DepartmentPath(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<DepartmentPath> Create(string value, Department? parentDept)
        {
            value = parentDept == null ? value : $"{parentDept.Path}{SEPARATOR}{value}";
            return new DepartmentPath(value);
        }
    }
}

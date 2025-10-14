namespace DirectoryService.Domain.Department
{
    public class DepartmentPath
    {
        private const char SEPARATOR = '.';

        public DepartmentPath(string value, Department? parentDept)
        {
            Value = parentDept == null ? value : $"{parentDept.Path}{SEPARATOR}{value}";
        }

        public string Value { get; }
    }
}

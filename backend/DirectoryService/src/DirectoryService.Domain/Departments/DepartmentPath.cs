using SharedKernel.Result;

namespace DirectoryService.Domain.Departments
{
    public sealed record DepartmentPath
    {
        private const char SEPARATOR = '.';
        private const string DELETED_MARK = "deleted_";

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

        public static Result<DepartmentPath> CreateForSoftDelete(DepartmentIdentifier identifier, Department? parentDept = null)
        {
            var value = identifier.Value;
            value = parentDept == null ? $"{DELETED_MARK}{value}" : $"{parentDept.Path.Value}{SEPARATOR}{DELETED_MARK}{value}";
            return new DepartmentPath(value);
        }
    }
}

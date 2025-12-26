namespace DirectoryService.Domain.Departments
{
    public sealed record DepartmentId
    {
        private DepartmentId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; } = Guid.Empty;

        public static DepartmentId Create() => new(Guid.NewGuid());
        public static DepartmentId Current(Guid id) => new(id);
    }
}

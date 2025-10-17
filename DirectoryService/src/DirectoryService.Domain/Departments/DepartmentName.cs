using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments
{
    public record DepartmentName
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
                return Result.Failure<DepartmentName>("Свойство \"Name\" не должно быть пустым");
            }

            if (name.Length < LengthConstants.LENGTH_3 || name.Length > LengthConstants.LENGTH_150)
            {
                return Result.Failure<DepartmentName>($"Свойство \"Name\" не должно быть меньше {LengthConstants.LENGTH_3} или больше {LengthConstants.LENGTH_150} символов");
            }

            return new DepartmentName(name);
        }
    }
}

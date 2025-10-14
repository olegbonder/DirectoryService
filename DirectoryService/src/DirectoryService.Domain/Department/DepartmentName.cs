namespace DirectoryService.Domain.Department
{
    public class DepartmentName
    {
        private const int MIN_LENGTH = 3;
        private const int MAX_LENGTH = 150;

        public DepartmentName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Свойство \"Name\" не должно быть пустым");
            }

            if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            {
                throw new ArgumentException($"Свойство \"Name\" не должно быть меньше {MIN_LENGTH} или больше {MAX_LENGTH} символов");
            }

            Value = value;
        }

        public string Value { get; }
    }
}

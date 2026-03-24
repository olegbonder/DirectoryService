using SharedKernel.Result;

namespace FileService.Domain
{
    public sealed record FileName
    {
        public string Value { get; }
        public string Name { get; }

        public string Extension { get; }

        private FileName(string name, string extension)
        {
            Name = name;
            Extension = extension;
            Value = name + "." + extension;
        }

        public static Result<FileName> Create(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return GeneralErrors.PropertyIsEmpty(nameof(fileName), "file name");
            }

            int lastDot = fileName.LastIndexOf('.');
            if (lastDot == -1 || lastDot == fileName.Length - 1)
            {
                return Error.Validation("extension.invalid", "File must have extension.");
            }

            string extension = fileName[(lastDot + 1)..].ToLowerInvariant();
            return new FileName(fileName, extension);
        }
    }
}
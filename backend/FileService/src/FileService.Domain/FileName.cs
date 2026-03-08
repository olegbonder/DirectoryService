using SharedKernel.Result;

namespace FileService.Domain
{
    public sealed record FileName
    {
        public string Name { get; }

        public string Extension { get; }

        private FileName(string name, string extension)
        {
            Name = name;
            Extension = extension;
        }

        private static Result<FileName> Create(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return GeneralErrors.ValueIsRequired(nameof(fileName));
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
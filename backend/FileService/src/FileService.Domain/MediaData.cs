using SharedKernel.Result;

namespace FileService.Domain
{
    public sealed record MediaData
    {
        // EF-Core
        private MediaData()
        {
        }

        public FileName FileName { get; }

        public ContentType ContentType { get; }

        public long Size { get; }

        public int ExpectedChunksCount { get; }

        public MediaData(FileName fileName, ContentType contentType, long size, int expectedChunksCount)
        {
            FileName = fileName;
            ContentType = contentType;
            Size = size;
            ExpectedChunksCount = expectedChunksCount;
        }

        public static Result<MediaData> Create(FileName fileName, ContentType contentType, long size, int expectedChunksCount)
        {
            if (size <= 0)
            {
                return GeneralErrors.ValueIsRequired("size");
            }

            if (expectedChunksCount <= 0)
            {
                return GeneralErrors.ValueIsRequired("expectedChunksCount");
            }

            return new MediaData(fileName, contentType, size, expectedChunksCount);
        }
    }
}
using FileService.Core.FilesStorage;
using Microsoft.Extensions.Options;
using SharedKernel.Result;

namespace FileService.Infrastructure.S3
{
    public class ChunkSizeCalculator : IChunkSizeCalculator
    {
        private readonly FileStorageOptions _options;

        public ChunkSizeCalculator(IOptions<FileStorageOptions> options)
        {
            _options = options.Value;
        }

        public Result<(int ChunkSize, int TotalChunks)> CalculateChunkSize(long fileSize)
        {
            if (_options.RecommendedChunkSizeBytes <= 0 || _options.MaxChunks <= 0)
                return GeneralErrors.ValueIsRequired("настройки чанков");

            if (fileSize <= _options.RecommendedChunkSizeBytes)
                return ((int)fileSize, 1);

            int calculatedChunks = (int)Math.Ceiling((double)fileSize / _options.RecommendedChunkSizeBytes);

            int totalChunks = Math.Min(calculatedChunks, _options.MaxChunks);

            long chunkSize = (fileSize + totalChunks - 1) / totalChunks;

            return ((int)chunkSize, totalChunks);
        }
    }
}
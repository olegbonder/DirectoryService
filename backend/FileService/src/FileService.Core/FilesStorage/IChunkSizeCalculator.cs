using SharedKernel.Result;

namespace FileService.Core.FilesStorage
{
    public interface IChunkSizeCalculator
    {
        Result<(int ChunkSize, int TotalChunks)> CalculateChunkSize(long fileSize);
    }
}
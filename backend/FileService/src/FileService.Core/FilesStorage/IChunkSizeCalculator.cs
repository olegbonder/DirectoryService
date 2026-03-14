using SharedKernel.Result;

namespace FileService.Core.FilesStorage
{
    public interface IChunkSizeCalculator
    {
        Result<(long ChunkSize, int TotalChunks)> CalculateChunkSize(long fileSize);
    }
}
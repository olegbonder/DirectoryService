using FileService.Domain;
using FileService.Domain.MediaProcessing;
using SharedKernel.Result;

namespace FileService.VideoProcessing.FfmpegProcess
{
    public interface IFfmpegProcessRunner
    {
        Task<Result<VideoMetaData>> ExtractMetadataAsync(
            string inputFileUrl,
            CancellationToken cancellationToken);

        Task<Result> GenerateHlsAsync(
            string inputFileUrl,
            string outputDirectory,
            CancellationToken cancellationToken);
    }
}
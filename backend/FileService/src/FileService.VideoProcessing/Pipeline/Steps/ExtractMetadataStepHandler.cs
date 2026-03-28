using FileService.Core.FilesStorage;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.VideoProcessing.Pipeline.Steps
{
    public sealed class ExtractMetadataStepHandler : IProcessingStepHandler
    {
        private readonly IFfmpegProcessRunner _ffmpegProcessRunner;
        private readonly IS3Provider _s3Provider;
        private readonly ILogger<ExtractMetadataStepHandler> _logger;

        public StepType StepType => StepType.EXTRACT_METADATA;

        public ExtractMetadataStepHandler(
            IFfmpegProcessRunner ffmpegProcessRunner,
            IS3Provider s3Provider,
            ILogger<ExtractMetadataStepHandler> logger)
        {
            _ffmpegProcessRunner = ffmpegProcessRunner;
            _s3Provider = s3Provider;
            _logger = logger;
        }

        public async Task<Result<ProcessingContext>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Extracting metadata for video asset {VideoAssetId}.", context.VideoProcess.Id);

            var inputFileUrlResult = await _s3Provider.GenerateDownloadUrlAsync(context.VideoAsset.UploadKey!);
            if (inputFileUrlResult.IsFailure)
                return inputFileUrlResult.Errors;

            string inputFileUrl = inputFileUrlResult.Value;
            context.SetMediaAssetUrl(inputFileUrl);

            var metadataResult = await _ffmpegProcessRunner.ExtractMetadataAsync(inputFileUrl, cancellationToken);
            if (metadataResult.IsFailure)
                return metadataResult.Errors;

            context.VideoProcess.SetMetaData(metadataResult.Value);

            return context;
        }
    }
}
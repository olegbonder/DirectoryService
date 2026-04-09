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
        private readonly IFileStorageProvider _fileStorageProvider;
        private readonly ILogger<ExtractMetadataStepHandler> _logger;

        public StepType StepType => StepType.EXTRACT_METADATA;

        public ExtractMetadataStepHandler(
            IFfmpegProcessRunner ffmpegProcessRunner,
            IFileStorageProvider fileStorageProvider,
            ILogger<ExtractMetadataStepHandler> logger)
        {
            _ffmpegProcessRunner = ffmpegProcessRunner;
            _fileStorageProvider = fileStorageProvider;
            _logger = logger;
        }

        public async Task<Result<ProcessingContext>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Extracting metadata for video asset {VideoAssetId}.", context.VideoProcess.Id);

            var inputFileUrlResult = await _fileStorageProvider.GenerateDownloadUrlAsync(context.VideoAsset.UploadKey!);
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
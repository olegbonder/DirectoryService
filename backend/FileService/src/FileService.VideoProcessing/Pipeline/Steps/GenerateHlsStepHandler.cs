using FileService.Core.FilesStorage;
using FileService.Domain;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.VideoProcessing.Pipeline.Steps
{
    public sealed class GenerateHlsStepHandler : IProcessingStepHandler
    {
        private readonly IFfmpegProcessRunner _ffmpegProcessRunner;
        private readonly IS3Provider _s3Provider;
        private readonly ILogger<GenerateHlsStepHandler> _logger;

        public StepType StepType => StepType.GENERATE_HLS;

        public GenerateHlsStepHandler(
            IFfmpegProcessRunner ffmpegProcessRunner,
            IS3Provider s3Provider,
            ILogger<GenerateHlsStepHandler> logger)
        {
            _ffmpegProcessRunner = ffmpegProcessRunner;
            _s3Provider = s3Provider;
            _logger = logger;
        }

        public async Task<Result<ProcessingContext>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating HLS for video asset {VideoAssetId}.", context.VideoProcess.Id);

            string inputFileUrl;

            if (!string.IsNullOrWhiteSpace(context.MediaAssetUrl))
            {
                inputFileUrl = context.MediaAssetUrl!;
            }
            else
            {
                _logger.LogDebug(
                    "InputFileUrl not found, generating new presigned url for video asset {VideoAssetId}.",
                    context.VideoProcess.Id);

                var urlResult = await _s3Provider.GenerateDownloadUrlAsync(context.VideoAsset.UploadKey!);
                if (urlResult.IsFailure)
                    return urlResult.Errors;

                inputFileUrl = urlResult.Value;
            }

            if (string.IsNullOrWhiteSpace(context.HlsOutputDirectory))
            {
                return FileErrors.HlsProcessingFailed("HLS output directory not found");
            }

            if (context.VideoProcess.MetaData is null)
            {
                _logger.LogWarning(
                    "MetaData is null, progress tracking will be disabled for video asset {VideoAssetId}.",
                    context.VideoProcess.Id);
            }

            var result = await _ffmpegProcessRunner
                .GenerateHlsAsync(inputFileUrl, context.HlsOutputDirectory, cancellationToken);
            if (result.IsFailure)
                return result.Errors;

            return context;
        }
    }
}
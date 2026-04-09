using FileService.Core.FilesStorage;
using FileService.Domain;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Result;

namespace FileService.VideoProcessing.Pipeline.Steps
{
    public sealed class UploadeHlsStepHandler : IProcessingStepHandler
    {
        private readonly IFfmpegProcessRunner _ffmpegProcessRunner;
        private readonly IFileStorageProvider _fileStorageProvider;
        private readonly ILogger<GenerateHlsStepHandler> _logger;
        private readonly VideoProcessingOptions _options;

        public StepType StepType => StepType.UPLOAD_HLS;

        public UploadeHlsStepHandler(
            IFfmpegProcessRunner ffmpegProcessRunner,
            IFileStorageProvider fileStorageProvider,
            ILogger<GenerateHlsStepHandler> logger,
            IOptions<VideoProcessingOptions> options)
        {
            _ffmpegProcessRunner = ffmpegProcessRunner;
            _fileStorageProvider = fileStorageProvider;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<Result<ProcessingContext>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Uploading HLS to S3 for video asset {VideoAssetId}.", context.VideoProcess.Id);

            if (string.IsNullOrWhiteSpace(context.HlsOutputDirectory))
                return FileErrors.HlsProcessingFailed("HLS output directory not set");

            if (!Directory.Exists(context.HlsOutputDirectory))
                return FileErrors.HlsProcessingFailed("HLS output directory not exists");

            string[] hlsFiles = Directory.GetFiles(context.HlsOutputDirectory, "*", SearchOption.AllDirectories);

            if (hlsFiles.Length == 0)
                return FileErrors.HlsProcessingFailed("HLS output directory is empty");

            var hlsRootKey = context.VideoAsset.GetHlsRootKey();
            if (hlsRootKey.IsFailure)
                return hlsRootKey.Errors;

            using var throttler = new SemaphoreSlim(_options.UploadDegreeOfParallelism);

            Task<Result>[] uploadTasks = hlsFiles.Select(async file =>
            {
                await throttler.WaitAsync(cancellationToken);
                try
                {
                    return await UploadHlsFileAsync(hlsRootKey.Value, file, cancellationToken);
                }
                finally
                {
                    throttler.Release();
                }
            }).ToArray();

            var results = await Task.WhenAll(uploadTasks);
            var errorResults = results.Where(x => x.IsFailure).ToList();
            if (errorResults.Any())
                return errorResults.First().Errors;

            _logger.LogInformation(
                "Successfully uploaded {FileCount} HLS to S3 for video asset {VideoAssetId}.",
                hlsFiles.Length,
                context.VideoProcess.Id);

            var masterPlaylistKeyResult = context.VideoAsset.GetHlsMasterPlaylistKey();
            if (masterPlaylistKeyResult.IsFailure)
                return masterPlaylistKeyResult.Errors;

            var setKeyResult = context.VideoAsset.SetHlsMasterPlaylistKey(masterPlaylistKeyResult.Value);
            if (setKeyResult.IsFailure)
                return setKeyResult.Errors;

            return context;
        }

        private async Task<Result> UploadHlsFileAsync(
            StorageKey hlsRootKey,
            string localFilePath,
            CancellationToken cancellationToken)
        {
            string fileName = Path.GetFileName(localFilePath);
            var storageKeyResult = hlsRootKey.AppendSegment(fileName);
            if (storageKeyResult.IsFailure)
                return storageKeyResult.Errors;

            string contentType = GetContentType(localFilePath);

            await using FileStream fileStream = File.OpenRead(localFilePath);

            return await _fileStorageProvider.UploadFileAsync(
                storageKeyResult.Value,
                fileStream,
                contentType,
                cancellationToken);
        }

        private string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".m3u8" => "application/vnd.apple.mpegurl",
                ".ts" => "video/mp2t",
                _ => "application/octet-stream"
            };
        }
    }
}
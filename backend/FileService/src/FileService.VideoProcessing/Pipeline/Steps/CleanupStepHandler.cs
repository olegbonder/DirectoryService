using FileService.Core.FilesStorage;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.VideoProcessing.Pipeline.Steps
{
    public sealed class CleanupStepHandler : IProcessingStepHandler
    {
        private readonly IFileStorageProvider _fileStorageProvider;
        private readonly ILogger<CleanupStepHandler> _logger;

        public StepType StepType => StepType.CLEANUP;

        public CleanupStepHandler(
            IFileStorageProvider fileStorageProvider,
            ILogger<CleanupStepHandler> logger)
        {
            _fileStorageProvider = fileStorageProvider;
            _logger = logger;
        }

        public async Task<Result<ProcessingContext>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cleanup temporary files for video asset {VideoAssetId}.", context.VideoProcess.Id);

            if (string.IsNullOrWhiteSpace(context.WorkingDirectory))
            {
                _logger.LogWarning("Working directory is empty, skipping cleanup for video asset {VideoAssetId}.", context.VideoProcess.Id);
                return await Task.FromResult(context);
            }

            var deleteResult = await _fileStorageProvider.DeleteFileAsync(context.VideoAsset.RawKey, cancellationToken);
            if (deleteResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to delete raw file for video asset {VideoAssetId}. Error: {Error}.",
                    context.VideoProcess.Id, deleteResult.Errors);
            }
            else
            {
                _logger.LogDebug(
                    "Deleted raw file for video asset {VideoAssetId}.",
                    context.VideoProcess.Id);
            }

            try
            {
                if (Directory.Exists(context.WorkingDirectory))
                {
                    Directory.Delete(context.WorkingDirectory, true);
                    _logger.LogDebug("Deleted working directory {WorkingDirectory}", context.WorkingDirectory);

                    context.Cleanup();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(exception: ex,
                    "Failed to delete working directory {WorkingDirectory}.",
                    context.WorkingDirectory);
            }

            return await Task.FromResult(context);
        }
    }
}
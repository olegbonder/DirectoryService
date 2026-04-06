using FileService.Core.Features.DeleteFile;
using IntegrationEvents.Directory.Events;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features.Messaging
{
    public class MediaAssetDepartmentDeletedHandler
    {
        public static async Task Handle(
            DepartmentDeleted message,
            DeleteFileHandler handler,
            ILogger<MediaAssetDepartmentDeletedHandler> logger,
            CancellationToken cancellationToken)
        {
            var mediaAssetId = message.MediaAssetId;
            var command = new DeleteFileCommand(mediaAssetId);
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                var firstError = result.Errors.First();
                string error = $"code: {firstError.Code} , message: {firstError.Message}";
                logger.LogWarning($"{error}. MediaAssetId={mediaAssetId}");
                return;
            }

            logger.LogInformation(
                "Deleted mediaAssetId={MediaAssetId} for {Context} with id={EntityId}",
                mediaAssetId,
                message.Context,
                message.EntityId);
        }
    }
}
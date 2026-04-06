using FileService.Core.Features.DeleteFile;
using IntegrationEvents.Directory.Events;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.Messaging
{
    public class MediaAssetDepartmentsDeletedHandler
    {
        public static async Task Handle(
            List<DepartmentDeleted> message,
            DeleteFileHandler handler,
            ILogger<MediaAssetDepartmentDeletedHandler> logger,
            CancellationToken cancellationToken)
        {
            if (message == null || message.Count == 0)
            {
                logger.LogWarning("No media asset departments deleted");
                return;
            }

            var results = new List<Result>();
            foreach (var department in message)
            {
                var mediaAssetId = department.MediaAssetId;
                var command = new DeleteFileCommand(mediaAssetId);
                results.Add(await handler.Handle(command, cancellationToken));
            }

            var failedResults = results.Where(r => r.IsFailure).ToList();
            if (failedResults.Any())
            {
                var errors = new Errors(failedResults.SelectMany(f => f.Errors.Select(e => e)));
                string error = string.Join(Environment.NewLine, errors.Select(error => error.Message));
                logger.LogWarning(error);
                return;
            }

            var mediaAssetIds = message.Select(department => department.MediaAssetId);
            logger.LogInformation(
                "Deleted mediaAssetIds={MediaAssetIds}",
                string.Join("; ", mediaAssetIds));
        }
    }
}
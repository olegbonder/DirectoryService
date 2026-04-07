using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Departments;
using IntegrationEvents.Files.Events;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Features.Departments.Messaging
{
    public class DepartmentVideoCreatedHandler
    {
        public static async Task Handle(
            VideoCreated message,
            IDepartmentsRepository departmentsRepository,
            ITransactionManager transactionManager,
            ILogger<DepartmentVideoCreatedHandler> logger,
            CancellationToken cancellationToken)
        {
            if (!message.EntityType.Equals(
                    nameof(Department).ToLowerInvariant(),
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var departmentId = DepartmentId.Current(message.EntityId);
            var department = await departmentsRepository
                .GetActiveDepartmentById(departmentId, cancellationToken);

            if (department == null)
            {
                logger.LogWarning(
                    "Department {DepartmentId} was not found for VideoCreated event. VideoId={VideoId}",
                    message.EntityId,
                    message.VideoId);
                return;
            }

            department.UpdateVideo(message.VideoId);

            var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                string error = string.Join(";", saveResult.Errors.Select(x => x.Message).ToList());
                logger.LogError("Error saving video {VideoId}. Error: {Error}", message.VideoId, error);
                return;
            }

            logger.LogInformation(
                "Attached video {VideoId} to department {DepartmentId}",
                message.VideoId,
                message.EntityId);
        }
    }
}
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Departments;
using IntegrationEvents.Files.Events;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Features.Departments.Messaging
{
    public class DepartmentPreviewCreatedHandler
    {
        public static async Task Handle(
            PreviewCreated message,
            IDepartmentsRepository departmentsRepository,
            ITransactionManager transactionManager,
            ILogger<DepartmentPreviewCreatedHandler> logger,
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
                    "Department {DepartmentId} was not found for PreviewCreated event. PreviewId={PreviewId}",
                    message.EntityId,
                    message.PreviewId);
                return;
            }

            department.UpdatePreview(message.PreviewId);

            var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                string error = string.Join(";", saveResult.Errors.Select(x => x.Message).ToList());
                logger.LogError("Error saving preview {PreviewId}. Error: {Error}", message.PreviewId, error);
                return;
            }

            logger.LogInformation(
                "Attached preview {PreviewId} to department {DepartmentId}",
                message.PreviewId,
                message.EntityId);
        }
    }
}
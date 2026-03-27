using FileService.Domain.Shared;
using SharedKernel.Result;

namespace FileService.Domain.MediaProcessing
{
    public sealed class VideoProcessStep
    {
        public Guid Id { get; private set; }

        public Guid ProcessId { get; private set; }

        public VideoProcessStepOrder Order { get; private set; }

        public VideoProcessStepName Name { get; private set; }

        public VideoProcessStatus Status { get; private set; }

        public VideoProcessProgress Progress { get; private set; }

        public DateTime? StartedAt { get; private set; }

        public DateTime? CompletedAt { get; private set; }

        public string? Error { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public VideoProcessStep(
            VideoProcessStepName name,
            VideoProcessStepOrder order)
        {
            Id = Guid.NewGuid();
            Name = name;
            Order = order;
            Progress = VideoProcessProgress.Create(VideoProcessProgress.MIN_LENGTH);
            Status = VideoProcessStatus.PENDING;
            CreatedAt = DateTime.Now;
            UpdatedAt = CreatedAt;
        }

        // EF Core
        private VideoProcessStep()
        {
        }

        public Result Start()
        {
            if (Status != VideoProcessStatus.PENDING)
            {
                return VideoProcessStepErrors.InvalidStatus(
                    $"Can only start step with status PENDING, current status: {Status}");
            }

            Status = VideoProcessStatus.RUNNING;
            Error = string.Empty;
            StartedAt = DateTime.UtcNow;
            UpdatedAt = StartedAt.Value;

            return Result.Success();
        }

        public Result SetProgress(double percent)
        {
            if (Status is VideoProcessStatus.SUCCEEDED or VideoProcessStatus.FAILED or VideoProcessStatus.CANCELED)
            {
                return VideoProcessStepErrors.InvalidStatus(
                    $"Can set step progress with status RUNNING, current status: {Status}");
            }

            Progress = VideoProcessProgress.Create(percent);
            if (Status == VideoProcessStatus.PENDING)
                Status = VideoProcessStatus.RUNNING;

            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Complete()
        {
            if (Status != VideoProcessStatus.RUNNING)
            {
                return VideoProcessStepErrors.InvalidStatus(
                    $"Can complete step with status RUNNING, current status: {Status}");
            }

            Progress = VideoProcessProgress.Create(VideoProcessProgress.MAX_LENGTH);
            Status = VideoProcessStatus.SUCCEEDED;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = CompletedAt.Value;

            return Result.Success();
        }

        public Result Fail(string error)
        {
            if (Status != VideoProcessStatus.RUNNING)
            {
                return VideoProcessStepErrors.InvalidStatus(
                    $"Can fail step with status RUNNING, current status: {Status}");
            }

            if (string.IsNullOrWhiteSpace(error))
            {
                return SharedKernel.Result.Error
                    .Failure("step.error.required", "Error message is required");
            }

            Status = VideoProcessStatus.FAILED;
            Error = error;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = CompletedAt.Value;

            return Result.Success();
        }

        public void Reset()
        {
            Status = VideoProcessStatus.PENDING;
            StartedAt = null;
            CompletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public enum VideoProcessStatus
    {
        PENDING,    // Ожидает запуска
        RUNNING,    // Выполняется
        SUCCEEDED,  // Успешно завершен
        FAILED,     // Завершен с ошибкой
        CANCELED // Отменен вручную
    }
}
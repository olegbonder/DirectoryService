using FileService.Domain.Shared;
using SharedKernel.Result;

namespace FileService.Domain.MediaProcessing
{
    public sealed class VideoProcess
    {
        private readonly List<VideoProcessStep> _steps = [];

        public Guid Id { get; private set; }

        public StorageKey RawKey { get; private set; }

        public StorageKey HlsKey { get; private set; }

        public VideoProcessStatus Status { get; private set; }

        public double TotalProgress { get; private set; }

        public VideoMetaData? MetaData { get; set; }

        public string? ErrorMessage { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public bool IsCompleted =>
            Status is VideoProcessStatus.SUCCEEDED or VideoProcessStatus.FAILED or VideoProcessStatus.CANCELED;

        public IReadOnlyList<VideoProcessStep> Steps => _steps.AsReadOnly();

        private VideoProcessStep? CurrentStep => _steps
            .FirstOrDefault(x => x.Status == VideoProcessStatus.RUNNING);

        public int? CurrentStepOrder => CurrentStep?.Order.Value;

        public string? CurrentStepName => CurrentStep?.Name.Value;

        public double? CurrentStepProgress => CurrentStep?.Progress.Value;

        // EF Core
        private VideoProcess()
        {
        }

        private VideoProcess(
            Guid id,
            StorageKey rawKey,
            StorageKey hlsKey,
            IEnumerable<VideoProcessStep> steps)
        {
            Id = id;
            RawKey = rawKey;
            HlsKey = hlsKey;
            Status = VideoProcessStatus.PENDING;
            TotalProgress = 0;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;

            _steps = steps.ToList();
        }

        public static Result<VideoProcess> Create(
            Guid id,
            StorageKey rawKey,
            StorageKey hlsKey,
            IEnumerable<VideoProcessStep> steps)
        {
            var videoProcessSteps = steps.ToList();
            if (videoProcessSteps.Count == 0)
            {
                return VideoProcessErrors.VideoProcessMustHaveMoreOneStep();
            }

            int uniqueOrderCount = videoProcessSteps.DistinctBy(s => s.Order.Value).Count();
            if (uniqueOrderCount != videoProcessSteps.Count)
            {
                return VideoProcessStepErrors.OrderMustBeUnique();
            }

            return new VideoProcess(id, rawKey, hlsKey, videoProcessSteps);
        }

        public static List<VideoProcessStep> CreateProcessingSteps()
        {
            var steps = new List<VideoProcessStep>();

            string[] stepNames = Enum.GetNames<StepType>();
            for (int i = 1; i <= stepNames.Length; i++)
            {
                var stepName = VideoProcessStepName.Create(stepNames[i - 1]);
                var stepOrder = VideoProcessStepOrder.Create(i);
                var step = new VideoProcessStep(stepName.Value, stepOrder.Value);
                steps.Add(step);
            }

            return steps;
        }

        public Result PrepareForExecution()
        {
            if (Status == VideoProcessStatus.CANCELED)
                return Error.Failure("processing.invalid.status", $"Cannot process step with status: {Status}");

            if (Status is VideoProcessStatus.RUNNING or VideoProcessStatus.FAILED)
            {
                foreach (var step in _steps)
                {
                    step.Reset();
                }

                CalculateTotalProgress();

                Status = VideoProcessStatus.PENDING;
                TotalProgress = 0;
            }

            Status = VideoProcessStatus.RUNNING;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result StartStep(int order, string name)
        {
            if (Status != VideoProcessStatus.RUNNING)
            {
                return VideoProcessStepErrors.InvalidStatus(
                    $"Cannot start step when process status is {Status}");
            }

            var videoStepOrderRes = VideoProcessStepOrder.Create(order);
            if (videoStepOrderRes.IsFailure)
                return videoStepOrderRes.Errors;

            var videoStepNameRes = VideoProcessStepName.Create(name);
            if (videoStepNameRes.IsFailure)
                return videoStepNameRes.Errors;

            var existsStep = _steps.FirstOrDefault(s => s.Name == videoStepNameRes.Value
                                                        && s.Order == videoStepOrderRes.Value);
            if (existsStep == null)
                return VideoProcessStepErrors.NotFound();

            var startStepResult = existsStep.Start();
            if (startStepResult.IsFailure)
                return startStepResult.Errors;

            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result CompleteStep(int order)
        {
            if (Status != VideoProcessStatus.RUNNING)
            {
                return VideoProcessStepErrors.InvalidStatus(
                    $"Cannot complete step when process status is {Status}");
            }

            var videoStepOrderRes = VideoProcessStepOrder.Create(order);
            if (videoStepOrderRes.IsFailure)
                return videoStepOrderRes.Errors;

            var existsStep = _steps.FirstOrDefault(s => s.Order == videoStepOrderRes.Value);
            if (existsStep == null)
                return VideoProcessStepErrors.NotFound();

            var completeStepResult = existsStep.Complete();
            if (completeStepResult.IsFailure)
                return completeStepResult.Errors;

            CalculateTotalProgress();

            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result ReportStepProgress(double percent)
        {
            if (CurrentStep == null)
            {
                return VideoProcessStepErrors.InvalidStatus(
                    "Cannot set percent when not exists steps with  status RUNNING");
            }

            var setProgressResult = CurrentStep.SetProgress(percent);
            if (setProgressResult.IsFailure)
                return setProgressResult.Errors;

            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result FinishProcessing()
        {
            bool allStepsSucceeded = _steps.All(x => x.Status == VideoProcessStatus.SUCCEEDED);
            if (!allStepsSucceeded)
            {
                return Error.Failure(
                    "processing.not.succeeded.status",
                    $"Can only success process when all steps are succeeded");
            }

            Status = VideoProcessStatus.SUCCEEDED;
            UpdatedAt = DateTime.UtcNow;
            TotalProgress = 100;

            return Result.Success();
        }

        public Result Fail(string message)
        {
            if (Status != VideoProcessStatus.RUNNING)
            {
                return VideoProcessStepErrors.InvalidStatus(
                    $"Cannot complete step when process status is {Status}");
            }

            if (string.IsNullOrWhiteSpace(message))
                return Error.Failure("processing.error.required", "Error message is required");

            Status = VideoProcessStatus.FAILED;
            ErrorMessage = message;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result Cancel(string message)
        {
            if (Status != VideoProcessStatus.RUNNING)
            {
                return VideoProcessStepErrors.InvalidStatus(
                    $"Cannot complete step when process status is {Status}");
            }

            if (string.IsNullOrWhiteSpace(message))
                return Error.Failure("processing.error.required", "Error message is required");

            Status = VideoProcessStatus.CANCELED;
            ErrorMessage = message;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public void SetMetaData(VideoMetaData metadata)
        {
            MetaData = metadata;
        }

        public Result<VideoProcessStep?> ProcessingNextStep()
        {
            if (Status != VideoProcessStatus.RUNNING)
                return VideoProcessStepErrors.InvalidStatus($"Cannot process step with status: {Status}");

            VideoProcessStep? currentStep = CurrentStep;
            if (currentStep is not null)
                return currentStep;

            VideoProcessStep? nextStep = _steps
                .OrderBy(x => x.Order)
                .FirstOrDefault(x => x.Status == VideoProcessStatus.RUNNING);

            if (nextStep is null)
            {
                FinishProcessing();
                return Result<VideoProcessStep?>.Success(null);
            }

            var startResult = nextStep.Start();
            if (startResult.IsFailure)
                return startResult.Errors;

            return nextStep;
        }

        public Result CompleteCurrentStep()
        {
            if (Status != VideoProcessStatus.RUNNING)
                return VideoProcessStepErrors.InvalidStatus($"Cannot complete step when status is {Status}");

            VideoProcessStep? currentStep = CurrentStep;
            if (currentStep is null)
                return Error.Failure("processing.no.active.step", "No active step to complete");

            var completeResult = CompleteStep(currentStep.Order.Value);

            return completeResult;
        }

        public Result FailCurrentStep(string errorMessage)
        {
            if (Status != VideoProcessStatus.RUNNING)
                return Error.Failure("processing.invalid.status", $"Cannot fail step when status is {Status}");

            VideoProcessStep? currentStep = CurrentStep;
            if (currentStep is null)
                return Error.Failure("processing.no.active.step", "No active step to fail");

            var failResult = currentStep.Fail(errorMessage);
            if (failResult.IsFailure)
                return failResult.Errors;

            return Result.Success();
        }

        private void CalculateTotalProgress()
        {
            int succeededSteps = _steps.Count(s => s.Status == VideoProcessStatus.SUCCEEDED);
            TotalProgress = (double)succeededSteps / _steps.Count * 100;
        }
    }
}
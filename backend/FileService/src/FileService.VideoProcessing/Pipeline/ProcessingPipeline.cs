using FileService.Core;
using FileService.Contracts.Dtos.VideoProcessing;
using FileService.Core.Database;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.VideoProcessing.Pipeline
{
    public class ProcessingPipeline : IProcessingPipeline
    {
        private readonly IEnumerable<IProcessingStepHandler> _stepHandlers;
        private readonly ILogger<ProcessingPipeline> _logger;
        private readonly IMediaAssetRepository _mediaAssetRepository;
        private readonly IVideoProcessingRepository _videoProcessingRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IVideoProgressReporter _progressReporter;

        public ProcessingPipeline(
            IEnumerable<IProcessingStepHandler> stepHandlers,
            ILogger<ProcessingPipeline> logger,
            IMediaAssetRepository mediaAssetRepository,
            IVideoProcessingRepository videoProcessingRepository,
            ITransactionManager transactionManager,
            IVideoProgressReporter progressReporter)
        {
            _stepHandlers = stepHandlers;
            _logger = logger;
            _mediaAssetRepository = mediaAssetRepository;
            _videoProcessingRepository = videoProcessingRepository;
            _transactionManager = transactionManager;
            _progressReporter = progressReporter;
        }

        public async Task<Result> ProcessAllStepsAsync(
            Guid videoAssetId,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await LoadContextAsync(videoAssetId, cancellationToken);
            if (contextResult.IsFailure)
                return contextResult.Errors;

            var context = contextResult.Value;

            var executionResult = await ExecuteAllStepsAsync(context, cancellationToken);

            if (executionResult.IsFailure)
            {
                var error = executionResult.Errors.First();
                if (error.Code == "pipeline.canceled" || error.Code == "operation.canceled" || error.Code == "process.canceled")
                {
                    return await FinalizeWithCancellationAsync(context, error, cancellationToken);
                }

                return await FinalizeWithFailureAsync(context, executionResult.Errors.First(), cancellationToken);
            }

            return await FinalizeAsync(context, cancellationToken);
        }

        private async Task<Result> FinalizeWithCancellationAsync(
            ProcessingContext context,
            Error error,
            CancellationToken cancellationToken)
        {
            var videoAssetId = context.VideoProcess.Id;
            var cancelResult = context.VideoProcess.Cancel(error.Message);
            if (cancelResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to cancel video process {VideoAssetId} after cancellation signal.",
                    videoAssetId);
                return cancelResult.Errors;
            }

            _logger.LogInformation(
                "Processing canceled for video asset {VideoAssetId}.",
                videoAssetId);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to save canceled state for video asset {VideoAssetId}.",
                    videoAssetId);

                return saveResult.Errors;
            }

            _progressReporter.Cancel(context.VideoProcess);
            return Result.Failure(error);
        }

        private async Task<Result> FinalizeAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            var videoAssetId = context.VideoProcess.Id;

            context.VideoAsset.CompleteProcessing(DateTime.UtcNow);
            context.VideoProcess.FinishProcessing();

            _logger.LogInformation(
                "Processing completed for video asset {VideoAssetId}.",
                videoAssetId);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to save success state for video asset {VideoAssetId}",
                    videoAssetId);

                return saveResult.Errors;
            }

            _progressReporter.FinishProcessing(context.VideoProcess);
            return Result.Success();
        }

        private async Task<Result> FinalizeWithFailureAsync(
            ProcessingContext context,
            Error error,
            CancellationToken cancellationToken)
        {
            var videoAssetId = context.VideoProcess.Id;

            context.VideoProcess.Fail(error.Message);

            _logger.LogError(
                "Processing failed for video asset {VideoAssetId}, Error: {Error}",
                videoAssetId,
                error.Message);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to save failure state for video asset {VideoAssetId}",
                    videoAssetId);

                return saveResult.Errors;
            }

            _progressReporter.Fail(context.VideoProcess);
            return Result.Failure(error);
        }

        private async Task<Result> ExecuteAllStepsAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            var videoAssetId = context.VideoProcess.Id;
            while (true)
            {
                var stepResult = context.VideoProcess.ProcessingNextStep();
                if (stepResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Failed to process next step for video asset {VideoAssetId}, Status: {Status}",
                        videoAssetId,
                        context.VideoProcess.Status);
                    return stepResult.Errors;
                }

                if (stepResult.Value == null)
                {
                    _logger.LogInformation(
                        "All steps processed for video asset {VideoAssetId}, Status: {Status}",
                        videoAssetId,
                        context.VideoProcess.Status);
                    return Result.Success();
                }

                var currentStep = stepResult.Value;

                _logger.LogInformation(
                    "Processing step {StepName} (Order: {StepOrder}) for video asset {VideoAssetId}",
                    currentStep.Name,
                    currentStep.Order,
                    videoAssetId);

                _progressReporter.StartStep(context.VideoProcess);

                var stepHandler = _stepHandlers
                    .FirstOrDefault(x => x.StepType.ToString() == currentStep.Name.Value);

                if (stepHandler == null)
                {
                    string error = $"No handler found for step {currentStep.Name.Value}";
                    _logger.LogError(
                        "No handler found for step {StepName} for video asset {VideoAssetId}",
                        currentStep.Name,
                        videoAssetId);

                    context.VideoProcess.FailCurrentStep(error);
                    context.VideoProcess.Fail(error);
                    var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                    if (saveResult.IsFailure)
                    {
                        _logger.LogError(
                            "Failed to save changes to database while failing step {StepName} for video asset {VideoAssetId}",
                            currentStep.Name,
                            videoAssetId);
                    }

                    return Error.Failure("pipeline.step.handler.not.found", error);
                }

                var executionResult = await ExecuteStepSafetyAsync(
                    stepHandler,
                    context,
                    cancellationToken);

                if (executionResult.IsFailure)
                {
                    _logger.LogError(
                        "Failed to execute step {StepName} for video asset {VideoAssetId}, Error: {ErrorMessage}",
                        currentStep.Name,
                        videoAssetId,
                        executionResult.Errors);

                    context.VideoProcess.FailCurrentStep(executionResult.Errors.First().Message);
                    context.VideoProcess.Fail(executionResult.Errors.First().Message);

                    var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                    if (saveResult.IsFailure)
                    {
                        _logger.LogError(
                            "Failed to save changes to database while failing step {StepName} for video asset {VideoAssetId}",
                            currentStep.Name,
                            videoAssetId);
                    }

                    _progressReporter.Fail(context.VideoProcess);

                    return executionResult.Errors;
                }

                var progressReportResult = context.VideoProcess.ReportStepProgress(context.VideoProcess.CurrentStepProgress ?? 0);
                if (progressReportResult.IsSuccess)
                {
                    _progressReporter.ReportStepProgress(context.VideoProcess);
                }

                var completeStepResult = context.VideoProcess.CompleteCurrentStep();
                if (completeStepResult.IsFailure)
                    return completeStepResult.Errors;

                _logger.LogInformation(
                    "Completed step {StepName} (Order: {StepOrder}) for video asset {VideoAssetId}, Progress: {Progress}",
                    currentStep.Name,
                    currentStep.Order,
                    videoAssetId,
                    context.VideoProcess.TotalProgress);

                var completeSaveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (completeSaveResult.IsFailure)
                {
                    _logger.LogError(
                        "Failed to save changes to database after step {StepName} for video asset {VideoAssetId}",
                        currentStep.Name,
                        videoAssetId);
                }

                _progressReporter.CompleteStep(context.VideoProcess);
            }
        }

        private async Task<Result<ProcessingContext>> ExecuteStepSafetyAsync(
            IProcessingStepHandler step,
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                return await step.ExecuteAsync(context, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Error.Failure("pipeline.canceled", "Video processing was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandle exception in step {Name} for video asset {VideoAssetId}",
                    context.VideoProcess.CurrentStepName,
                    context.VideoProcess.Id);

                return Error.Failure("pipeline.step.exception", $"Step execution failed: {ex.Message}");
            }
        }

        private async Task<Result<ProcessingContext>> LoadContextAsync(
            Guid videoAssetId,
            CancellationToken cancellationToken)
        {
            var processResult = await _videoProcessingRepository
                .GetBy(v => v.Id == videoAssetId, cancellationToken);

            VideoProcess videoProcess;

            if (processResult.IsFailure)
            {
                var videoAssetResult = await _mediaAssetRepository
                    .GetVideoBy(v => v.Id == videoAssetId, cancellationToken);
                if (videoAssetResult.IsFailure)
                    return videoAssetResult.Errors;

                var steps = VideoProcess.CreateProcessingSteps();
                var newProcess = VideoProcess.Create(
                    videoAssetId,
                    videoAssetResult.Value.RawKey,
                    videoAssetResult.Value.HlsRootKey,
                    steps);
                videoProcess = newProcess;

                _videoProcessingRepository.Add(newProcess);

                _logger.LogInformation("Created new process for video asset {VideoAssetId}.", videoAssetId);
            }
            else
            {
                videoProcess = processResult.Value;
                _logger.LogInformation("Found existing process for video asset {VideoAssetId}.", videoAssetId);
            }

            var assetResult = await _mediaAssetRepository
                .GetVideoBy(v => v.Id == videoAssetId, cancellationToken);
            if (assetResult.IsFailure)
                return assetResult.Errors;

            var videoAsset = assetResult.Value;

            if (!videoAsset.RequiredProcessing())
            {
                return Error.Validation(
                    "asset.processing.not.required",
                    "This asset does not require processing");
            }

            var executeProcessResult = videoProcess.PrepareForExecution();
            if (executeProcessResult.IsFailure)
                return executeProcessResult.Errors;

            _progressReporter.PrepareForExecution(videoProcess);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
                return saveResult.Errors;

            var processingContext = new ProcessingContext
            {
                VideoAsset = assetResult.Value,
                VideoProcess = videoProcess
            };

            return processingContext;
        }
    }
}
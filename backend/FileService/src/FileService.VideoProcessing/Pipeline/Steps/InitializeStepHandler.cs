using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.VideoProcessing.Pipeline.Steps
{
    public sealed class InitializeStepHandler : IProcessingStepHandler
    {
        private readonly ILogger<InitializeStepHandler> _logger;

        public StepType StepType => StepType.INITIALIZATE;

        public InitializeStepHandler(ILogger<InitializeStepHandler> logger)
        {
            _logger = logger;
        }

        public Task<Result<ProcessingContext>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Initializing video processing for video asset {VideoAssetId}.",
                context.VideoProcess.Id);

            var createResult = context.CreateWorkingDirectory();
            if (createResult.IsFailure)
                return Task.FromResult(Result<ProcessingContext>.Failure(createResult.Errors));

            return Task.FromResult(Result<ProcessingContext>.Success(context));
        }
    }
}
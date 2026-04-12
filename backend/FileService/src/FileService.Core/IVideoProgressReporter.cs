using FileService.Domain.MediaProcessing;

namespace FileService.Core;

public interface IVideoProgressReporter
{
    void PrepareForExecution(VideoProcess videoProcess);

    void StartStep(VideoProcess videoProcess);

    void ReportStepProgress(VideoProcess videoProcess);

    void CompleteStep(VideoProcess videoProcess);

    void Fail(VideoProcess videoProcess);

    void Cancel(VideoProcess videoProcess);

    void FinishProcessing(VideoProcess videoProcess);
}

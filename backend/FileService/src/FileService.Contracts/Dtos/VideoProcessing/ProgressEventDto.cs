using System.Text.RegularExpressions;
using FileService.Domain.MediaProcessing;

namespace FileService.Contracts.Dtos.VideoProcessing;

public sealed record ProgressEventDto(
    Guid MediaAssetId,
    string ProcessStatus,
    double Percent,
    int? StepOrder,
    string? StepName,
    int TotalSteps,
    string? Error,
    string? ErrorCode,
    DateTime PublishedAtUtc)
{
    public static ProgressEventDto FromVideoProcess(VideoProcess videoProcess)
    {
        var errorMessage = videoProcess.ErrorMessage;
        var failedStep = videoProcess.Steps.FirstOrDefault(x => x.Status == VideoProcessStatus.FAILED);
        var error = errorMessage ?? failedStep?.Error;

        return new ProgressEventDto(
            videoProcess.Id,
            NormalizeStatus(videoProcess.Status),
            videoProcess.TotalProgress,
            videoProcess.CurrentStepOrder,
            videoProcess.CurrentStepName,
            videoProcess.Steps.Count,
            error,
            NormalizeErrorCode(error),
            DateTime.UtcNow);
    }

    private static string NormalizeStatus(VideoProcessStatus status)
    {
        return status.ToString().ToLowerInvariant();
    }

    private static string? NormalizeErrorCode(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return null;

        if (error.Trim().Contains('.'))
            return error;

        return null;
    }
}

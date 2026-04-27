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
    DateTime PublishedAtUtc);

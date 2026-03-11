namespace FileService.Infrastructure.S3;

public record S3Options
{
    public string EndPoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public bool WithSsl { get; init; }
    public int DownloadExpirationHours { get; init; } = 24;

    public IReadOnlyList<string> RequiredBuckets { get; init; } = [];
    public double UploadUrlExpirationMinutes { get; init; } = 30;

    public int MaxConcurrentRequests { get; init; } = 20;
}
namespace FileService.Core.FilesStorage;

public record FileStorageOptions
{
    public string EndPoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public bool WithSsl { get; init; }
    public int DownloadExpirationHours { get; init; } = 24;

    public IReadOnlyList<string> RequiredBuckets { get; init; } = [];
    public double UploadUrlExpirationMinutes { get; init; } = 30;

    public int MaxConcurrentRequests { get; init; } = 20;

    public long RecommendedChunkSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
    public int MaxChunks { get; init; } = 10000;
}
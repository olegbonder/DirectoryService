using SharedKernel.Result;

namespace FileService.Domain.Assets;

public class VideoAsset: MediaAsset
{
    public StorageKey? HlsRootKey { get; protected set; }

    // EF Core
    private VideoAsset()
    {
    }

    private VideoAsset(
        Guid id,
        MediaData data,
        MediaStatus status,
        MediaOwner owner,
        StorageKey rawKey)
        : base(id, data, status, AssetType.VIDEO, owner, rawKey)
    {
    }

    public override bool RequiredProcessing() => true;

    public const string MASTER_PLAYLIST_NAME = "master.m3u8";
    public const string STREAM_PLAYLIST_PATTERN = "%v_stream.m3u8";
    public const string SEGMENT_FILE_PATTERN = "%v_%06d.ts";
    public const string HLS_PREFIX = "hls";

    private const long MAX_SIZE = 5_368_709_120; // 5 GB
    public const string BUCKET = "videos";
    private const string RAW_PREFIX = "raw";
    private const string ALLOWED_CONTENT_TYPE = "video";
    private static readonly string[] _allowedExtensions = ["mp4", "mkv", "avi", "mov"];

    public static Result<VideoAsset> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        var validationResult = ValidateForUpload(mediaData);
        if (validationResult.IsFailure)
        {
            return validationResult.Errors;
        }

        var keyResult = StorageKey.Create(BUCKET, RAW_PREFIX, id.ToString());
        if (keyResult.IsFailure)
            return keyResult.Errors;

        return new VideoAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            owner,
            keyResult.Value);
    }

    public Result<StorageKey> GetHlsRootKey()
    {
        return StorageKey.Create(BUCKET, HLS_PREFIX, Id.ToString());
    }

    public Result<StorageKey> GetHlsMasterPlaylistKey()
    {
        var hlsRootKey = GetHlsRootKey();
        if (hlsRootKey.IsFailure)
            return hlsRootKey.Errors;

        return hlsRootKey.Value.AppendSegment(MASTER_PLAYLIST_NAME);
    }

    public Result CompleteProcessing(DateTime timestamp)
    {
        if (FinalKey == null)
            return GeneralErrors.ValueIsRequired(nameof(FinalKey));

        var appendSegmentResult = FinalKey.AppendSegment(MASTER_PLAYLIST_NAME);
        if (appendSegmentResult.IsFailure)
            return appendSegmentResult.Errors;

        var markReadyResult = MarkReady(FinalKey, timestamp);
        if (markReadyResult.IsFailure)
            return markReadyResult.Errors;

        return Result.Success();
    }

    public Result SetHlsMasterPlaylistKey(StorageKey value)
    {
        if (Status != MediaStatus.UPLOADED)
            return Error.Validation("video.invalid.status", "Can only set hls master playlist key from UPLOADED status");

        if (FinalKey is not null)
            return Error.Validation("video.hls.key.exists", "HLS master playlist key already exists");

        FinalKey = value;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result ValidateForUpload(MediaData mediaData)
    {
        if (!_allowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation("video.invalid.extension", $"File extension must be one of: {string.Join(", ", _allowedExtensions)}");
        }

        if (mediaData.ContentType.Category != MediaType.VIDEO)
        {
            return Error.Validation("video.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");
        }

        if (mediaData.Size > MAX_SIZE)
        {
            return Error.Validation("video.invalid.size", $"File size must be less than {MAX_SIZE}");
        }

        return Result.Success();
    }
}
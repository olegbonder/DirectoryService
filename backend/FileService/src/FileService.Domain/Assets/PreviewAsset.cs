using SharedKernel.Result;

namespace FileService.Domain.Assets;

public class PreviewAsset: MediaAsset
{
    private PreviewAsset()
    {
    }

    private PreviewAsset(
        Guid id,
        MediaData data,
        MediaStatus status,
        MediaOwner owner,
        StorageKey key)
        : base(id, data, status, AssetType.PREVIEW, owner, key, true)
    {
    }

    private const long MAX_SIZE = 10_485_760; // 10 MB
    private const string BUCKET = "preview";
    private const string RAW_PREFIX = "raw";
    private const string ALLOWED_CONTENT_TYPE = "image";

    private static readonly string[] _allowedExtensions = ["jpg", "jpeg", "png", "webp"];

    public static Result ValidateForUpload(MediaData mediaData)
    {
        if (!_allowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation("image.invalid.extension", $"File extension must be one of: {string.Join(", ", _allowedExtensions)}");
        }

        if (mediaData.ContentType.Category != MediaType.IMAGE)
        {
            return Error.Validation("image.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");
        }

        if (mediaData.Size > MAX_SIZE)
        {
            return Error.Validation("image.invalid.size", $"File size must be less than {MAX_SIZE}");
        }

        return Result.Success();
    }

    public static Result<PreviewAsset> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        var validationResult = ValidateForUpload(mediaData);
        if (validationResult.IsFailure)
        {
            return validationResult.Errors;
        }

        var keyResult = StorageKey.Create(BUCKET, RAW_PREFIX, id.ToString());
        if (keyResult.IsFailure)
        {
            return keyResult.Errors;
        }

        return new PreviewAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            owner,
            keyResult.Value);
    }

    public Result CompleteUpload(DateTime timestamp)
    {
        var markUploadResult = MarkUploaded(timestamp);
        if (markUploadResult.IsFailure)
            return markUploadResult.Errors;

        var markReadyResult = MarkReady(RawKey, timestamp);
        if (markReadyResult.IsFailure)
            return markReadyResult.Errors;

        return Result.Success();
    }
}
namespace FileService.Contracts.MediaAssets.StartMultiPartUpload;

public record StartMultiPartUploadResponse(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<ChunkUploadUrl> ChunkUploadUrls,
    long ChunkSize);

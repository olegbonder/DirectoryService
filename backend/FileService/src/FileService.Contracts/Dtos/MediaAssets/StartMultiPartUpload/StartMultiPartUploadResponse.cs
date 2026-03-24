namespace FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;

public record StartMultiPartUploadResponse(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<ChunkUploadUrl> ChunkUploadUrls,
    int ChunkSize);

namespace FileService.Contracts.MediaAssets.GetChunkUploadUrl;

public record GetChunkUploadUrlRequest(Guid MediaAssetId, string UploadId, int PartNumber);
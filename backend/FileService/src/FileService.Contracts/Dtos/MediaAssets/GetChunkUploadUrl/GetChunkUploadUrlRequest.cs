namespace FileService.Contracts.Dtos.MediaAssets.GetChunkUploadUrl;

public record GetChunkUploadUrlRequest(Guid MediaAssetId, string UploadId, int PartNumber);
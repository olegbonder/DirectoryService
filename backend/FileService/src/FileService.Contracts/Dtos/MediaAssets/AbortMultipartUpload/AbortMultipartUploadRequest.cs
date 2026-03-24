namespace FileService.Contracts.Dtos.MediaAssets.AbortMultipartUpload
{
    public record AbortMultipartUploadRequest(Guid MediaAssetId, string UploadId);
}

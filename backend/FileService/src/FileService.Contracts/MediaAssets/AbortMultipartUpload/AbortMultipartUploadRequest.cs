namespace FileService.Contracts.MediaAssets.AbortMultipartUpload
{
    public record AbortMultipartUploadRequest(Guid MediaAssetId, string UploadId);
}

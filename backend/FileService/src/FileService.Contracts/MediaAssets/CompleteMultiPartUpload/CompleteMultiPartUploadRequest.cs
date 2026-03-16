namespace FileService.Contracts.MediaAssets.CompleteMultiPartUpload
{
    public record CompleteMultiPartUploadRequest(Guid MediaAssetId, string UploadId, IReadOnlyList<PartEtagDto> PartETags);
}

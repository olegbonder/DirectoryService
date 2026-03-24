namespace FileService.Contracts.Dtos.MediaAssets.CompleteMultiPartUpload
{
    public record CompleteMultiPartUploadRequest(Guid MediaAssetId, string UploadId, IReadOnlyList<PartEtagDto> PartETags);
}

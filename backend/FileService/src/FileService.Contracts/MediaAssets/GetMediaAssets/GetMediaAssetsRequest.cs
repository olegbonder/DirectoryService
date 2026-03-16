namespace FileService.Contracts.MediaAssets.GetMediaAssets
{
    public record GetMediaAssetsRequest(IReadOnlyList<Guid> MediaAssetIds);
}
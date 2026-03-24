namespace FileService.Contracts.Dtos.MediaAssets.GetMediaAssets
{
    public record GetMediaAssetsRequest(IReadOnlyList<Guid> MediaAssetIds);
}
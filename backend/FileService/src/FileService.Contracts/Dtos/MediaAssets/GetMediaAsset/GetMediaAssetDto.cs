namespace FileService.Contracts.Dtos.MediaAssets.GetMediaAsset
{
    public record GetMediaAssetResponse(
        Guid Id,
        string Status,
        string AssetType,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        FileInfoDto FileInfo,
        string? DownloadUrl);
}
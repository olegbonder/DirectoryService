namespace FileService.Contracts.MediaAssets.GetMediaAsset
{
    public record GetMediaAssetDto(
        Guid Id,
        string Status,
        string AssetType,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        FileInfoDto FileInfo,
        string? DownloadUrl);
}
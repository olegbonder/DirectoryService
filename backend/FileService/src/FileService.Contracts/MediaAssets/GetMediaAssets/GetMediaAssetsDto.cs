namespace FileService.Contracts.MediaAssets.GetMediaAssets
{
    public record GetMediaAssetsDto(
        Guid Id,
        string Status,
        string? DownloadUrl);
}
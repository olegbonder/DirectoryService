namespace FileService.Contracts.Dtos.MediaAssets.GetMediaAssets
{
    public record GetMediaAssetsDto(
        Guid Id,
        string Status,
        string? DownloadUrl);
}
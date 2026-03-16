namespace FileService.Contracts.MediaAssets.GetMediaAssets;

public record GetMediaAssetsResponse(IEnumerable<GetMediaAssetsDto> Items);
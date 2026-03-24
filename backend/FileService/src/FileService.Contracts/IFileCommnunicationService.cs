using FileService.Contracts.Dtos.MediaAssets.GetMediaAsset;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAssets;
using SharedKernel.Result;

namespace FileService.Contracts;

public interface IFileCommunicationService
{
    Task<Result<GetMediaAssetResponse>> GetMediaAssetInfo(Guid mediaAssetId, CancellationToken cancellationToken);

    Task<Result<GetMediaAssetsResponse>> GetMediaAssetsInfo(GetMediaAssetsRequest request, CancellationToken cancellationToken);
}
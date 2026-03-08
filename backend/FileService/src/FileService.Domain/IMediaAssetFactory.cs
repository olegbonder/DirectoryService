using FileService.Domain.Assets;
using SharedKernel.Result;

namespace FileService.Domain;

public interface IMediaAssetFactory
{
    Result<VideoAsset> CreateVideoForUpload(MediaData mediaData, MediaOwner owner);

    Result<PreviewAsset> CreatePreviewForUpload(MediaData mediaData, MediaOwner owner);
}
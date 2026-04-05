using FileService.Core.Database;
using FileService.Domain;
using FileService.Domain.Assets;
using IntegrationEvents.Files.Events;
using SharedKernel.Result;

namespace FileService.Core.Messaging;

public interface IAssetCreatedEventPublisher
{
    Task<Result> PublishAsync(MediaAsset asset);
}

public sealed class AssetCreatedEventPublisher : IAssetCreatedEventPublisher
{
    private readonly Dictionary<AssetType, Func<MediaAsset, Task>> _publishers;

    public AssetCreatedEventPublisher(IOutboxService outboxService)
    {
        _publishers = new Dictionary<AssetType, Func<MediaAsset, Task>>
        {
            [AssetType.VIDEO] = asset => outboxService.PublishAsync(
                new VideoCreated(asset.Id, asset.Owner.EntityId, asset.Owner.Context)),
            [AssetType.PREVIEW] = asset => outboxService.PublishAsync(
                new PreviewCreated(asset.Id, asset.Owner.EntityId, asset.Owner.Context)),
        };
    }

    public async Task<Result> PublishAsync(MediaAsset asset)
    {
        if (!_publishers.TryGetValue(asset.AssetType,  out var publisher))
        {
            return Error.Validation(
                "asset.unsupported.type",
                $"No integration event mapping for asset type '{asset.AssetType}'");
        }

        await publisher(asset);

        return Result.Success();
    }
}
using System.Linq.Expressions;
using FileService.Domain.Assets;
using SharedKernel.Result;

namespace FileService.Core.Repositories;

public interface IMediaAssetRepository
{
    Task<Result<Guid>> Add(MediaAsset mediaAsset, CancellationToken cancellationToken);

    Task<MediaAsset?> GetBy(Expression<Func<MediaAsset, bool>> predicate, CancellationToken cancellationToken);

    Task<Result<VideoAsset>> GetVideoBy(
        Expression<Func<VideoAsset, bool>> predicate,
        CancellationToken cancellationToken);

    Task<Result<MediaAsset>> GetById(Guid mediaAssetId, CancellationToken cancellationToken);

    Task<Result> Delete(Guid mediaAssetId, CancellationToken cancellationToken);
}
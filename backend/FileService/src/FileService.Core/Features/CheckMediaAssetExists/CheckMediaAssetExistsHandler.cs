using FileService.Contracts.Dtos.MediaAssets.CheckMediaAssetExists;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;

namespace FileService.Core.Features.CheckMediaAssetExists;

public sealed class CheckMediaAssetExistsHandler
{
    private readonly IReadDbContext _readDbContext;

    public CheckMediaAssetExistsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<CheckMediaAssetExistsResponse>> Handle(
        Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        bool exists = await _readDbContext.MediaAssetsQuery
            .AnyAsync(m => m.Id == mediaAssetId, cancellationToken);

        return new CheckMediaAssetExistsResponse(exists);
    }
}
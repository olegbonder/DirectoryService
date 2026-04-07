using System.Linq.Expressions;
using FileService.Core;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Infrastructure.Postgres.Repositories;

public class MediaAssetRepository : IMediaAssetRepository
{
    private readonly FileServiceDbContext _context;
    private readonly ILogger<MediaAssetRepository> _logger;

    public MediaAssetRepository(FileServiceDbContext context, ILogger<MediaAssetRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Add(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        var fileInfo = $"Файл: {mediaAsset.MediaData.FileName} Тип: {mediaAsset.AssetType} Путь к S3:{mediaAsset.UploadKey.FullPath}";
        try
        {
            await _context.MediaAssets.AddAsync(mediaAsset, cancellationToken);

            return mediaAsset.Id;
        }
        catch(OperationCanceledException ex)
        {
            _logger.LogError(ex, "Отмена операции добавления медиа-файла {fileInfo}", fileInfo);
            return GeneralErrors.OperationCancelled("create.media_asset");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка добавления медиа-файла {fileInfo}", fileInfo);
            return MediaAssetErrors.DatabaseError();
        }
    }

    public async Task<MediaAsset?> GetBy(
        Expression<Func<MediaAsset, bool>> predicate, CancellationToken cancellationToken) =>
        await _context.MediaAssets.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<Result<VideoAsset>> GetVideoBy(
        Expression<Func<VideoAsset, bool>> predicate, CancellationToken cancellationToken)
    {
        var videoAsset = await _context.MediaAssets
            .OfType<VideoAsset>().FirstOrDefaultAsync(predicate, cancellationToken);
        if (videoAsset == null)
            return GeneralErrors.NotFound("video_asset", null);

        return videoAsset!;
    }

    public async Task<Result<MediaAsset>> GetById(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        var mediaAsset = await GetBy(m => m.Id == mediaAssetId && m.Status != MediaStatus.DELETED, cancellationToken);
        if (mediaAsset == null)
            return GeneralErrors.NotFound("media_asset", mediaAssetId);

        return mediaAsset;
    }

    public async Task<Result> Delete(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        string fileInfo = $"Id: {mediaAssetId}";
        try
        {
            int deletedCount = await _context.MediaAssets
                .Where(m => m.Id == mediaAssetId)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount == 0)
                return GeneralErrors.NotFound("media_asset", mediaAssetId);

            _logger.LogInformation("Медиа-файл с {fileInfo} успешно удален", fileInfo);
            return Result.Success();
        }
        catch(OperationCanceledException ex)
        {
            _logger.LogError(ex, "Отмена операции удаления медиа-файла {fileInfo}", fileInfo);
            return GeneralErrors.OperationCancelled("delete.media_asset");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка удаления медиа-файла {fileInfo}", fileInfo);
            return MediaAssetErrors.DatabaseError();
        }
    }
}
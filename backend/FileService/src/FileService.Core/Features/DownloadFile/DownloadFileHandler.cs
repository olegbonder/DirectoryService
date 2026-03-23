using Core.Abstractions;
using FileService.Contracts.MediaAssets.DownloadFile;
using FileService.Core.FilesStorage;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Core.Features.DownloadFile;

public class DownloadFileHandler : IQueryHandler<DownloadFileResponse, DownloadFileRequest>
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<DownloadFileHandler> _logger;

    public DownloadFileHandler(
        IMediaAssetRepository mediaAssetRepository,
        IS3Provider s3Provider,
        ILogger<DownloadFileHandler> logger)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<Result<DownloadFileResponse>> Handle(
        DownloadFileRequest request, CancellationToken cancellationToken)
    {
        var fileId = request.FileId;
        if (fileId == Guid.Empty)
            return GeneralErrors.PropertyIsEmpty("FileId", "FileId");

        var mediaAssetResult = await _mediaAssetRepository.GetById(fileId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Errors;

        var mediaAsset = mediaAssetResult.Value;

        var downloadResult = await _s3Provider.DownloadFileAsync(
            mediaAsset.RawKey,
            "",
            cancellationToken);
        if (downloadResult.IsFailure)
        {
            return downloadResult.Errors;
        }

        _logger.LogInformation("Download file with id:{fileId}", fileId);

        return new DownloadFileResponse(downloadResult.Value);
    }
}
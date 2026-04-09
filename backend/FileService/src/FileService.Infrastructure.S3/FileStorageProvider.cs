using Amazon.S3;
using Amazon.S3.Model;
using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.ListMultipartUpload;
using FileService.Core.FilesStorage;
using FileService.Core.Models;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Result;

namespace FileService.Infrastructure.S3;

public class FileStorageProvider : IFileStorageProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<FileStorageProvider> _logger;
    private readonly FileStorageOptions _s3StorageOptions;

    private readonly SemaphoreSlim _requestsSemaphore;

    public FileStorageProvider(IAmazonS3 s3Client, IOptions<FileStorageOptions> s3Options, ILogger<FileStorageProvider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3StorageOptions = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(1, _s3StorageOptions.MaxConcurrentRequests);
    }

    public async Task<Result> UploadFileAsync(
        StorageKey storageKey,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                InputStream = stream,
                ContentType = contentType ?? "application/octet-stream"
            };
            await _s3Client.PutObjectAsync(request, cancellationToken);

            _logger.LogInformation($"Uploaded file to {storageKey.FullPath}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> DownloadFileAsync(StorageKey storageKey, string tempPath,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _s3Client.GetObjectAsync(
                storageKey.Bucket, storageKey.Value, cancellationToken);
            return result.Key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> DeleteFileAsync(StorageKey storageKey, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _s3Client.DeleteObjectAsync(
                storageKey.Bucket, storageKey.Value, cancellationToken);
            _logger.LogInformation($"Deleted file {storageKey.FullPath}");

            return Result<string>.Success(result.DeleteMarker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath}", storageKey.FullPath);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> GenerateUploadUrlAsync(
        StorageKey storageKey,
        MediaData mediaData,
        CancellationToken cancellationToken)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = storageKey.Bucket,
            Key = storageKey.Value,
            Verb = HttpVerb.PUT,
            ContentType = mediaData.ContentType.Value,
            Expires = DateTime.Now.AddMinutes(_s3StorageOptions.UploadUrlExpirationMinutes),
            Protocol = _s3StorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
        };
        try
        {
            string? result = await _s3Client.GetPreSignedURLAsync(request);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating upload url for {FullPath}", storageKey.FullPath);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> GenerateDownloadUrlAsync(StorageKey storageKey)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = storageKey.Bucket,
            Key = storageKey.Value,
            Verb = HttpVerb.GET,
            Expires = DateTime.Now.AddHours(_s3StorageOptions.DownloadExpirationHours),
            Protocol = _s3StorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
        };
        try
        {
            string? result = await _s3Client.GetPreSignedURLAsync(request);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download url for {FullPath}", storageKey.FullPath);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<MediaUrl>>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken)
    {
        try
        {
            var tasks = storageKeys.Select(async storageKey =>
            {
                await _requestsSemaphore.WaitAsync(cancellationToken);
                try
                {
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = storageKey.Bucket,
                        Key = storageKey.Value,
                        Verb = HttpVerb.GET,
                        Expires = DateTime.Now.AddHours(_s3StorageOptions.DownloadExpirationHours),
                        Protocol = _s3StorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };
                    string? result = await _s3Client.GetPreSignedURLAsync(request);
                    return new MediaUrl(storageKey, result);
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });
            MediaUrl[] results = await Task.WhenAll(tasks);
            return Result<IReadOnlyList<MediaUrl>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download urls");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> StartMultiPartUploadAsync(
        StorageKey storageKey,
        MediaData mediaData,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                ContentType = mediaData.ContentType.Value
            };
            var result = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);

            return result.UploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting multipart upload");

            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> GenerateChunkUploadUrlAsync(
        StorageKey storageKey,
        string uploadId,
        int partNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                Verb = HttpVerb.PUT,
                UploadId = uploadId,
                PartNumber = partNumber,
                Expires = DateTime.UtcNow.AddMinutes(_s3StorageOptions.UploadUrlExpirationMinutes),
                Protocol = _s3StorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP
            };
            string? url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chunk upload url");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<ChunkUploadUrl>>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            var tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        var request = new GetPreSignedUrlRequest
                        {
                            BucketName = storageKey.Bucket,
                            Key = storageKey.Value,
                            Verb = HttpVerb.PUT,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.AddMinutes(_s3StorageOptions.UploadUrlExpirationMinutes),
                            Protocol = _s3StorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP
                        };
                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return new ChunkUploadUrl(partNumber, url);
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });

            ChunkUploadUrl[] results = await Task.WhenAll(tasks);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error get chunks url multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> GenerateUploadUrlAsync(StorageKey storageKey)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(_s3StorageOptions.DownloadExpirationHours),
                Protocol = _s3StorageOptions.WithSsl ? Protocol.HTTPS : Protocol.HTTP
            };
            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating upload url");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result> CompleteMultiPartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartEtagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new PartETag { ETag = p.ETag, PartNumber = p.PartNumber }).ToList(),
            };

            await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing multipart upload for {uploadId}", uploadId);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result> AbortMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new AbortMultipartUploadRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                UploadId = uploadId
            };

            await _s3Client.AbortMultipartUploadAsync(request, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aborting multipart upload for {uploadId}", uploadId);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<MultipartUploadsResponse>> ListMultipartUploadAsync(string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            var request = new ListMultipartUploadsRequest
            {
                BucketName = bucketName
            };
            var multipartUploadsResponse = await _s3Client.ListMultipartUploadsAsync(request, cancellationToken);
            var multipartUploads = multipartUploadsResponse.MultipartUploads.Select(u => new MultipartUploadDto(u.UploadId, u.Key)).ToList();
            return new MultipartUploadsResponse(multipartUploads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing multipart uploads");
            return S3ErrorMapper.ToError(ex);
        }
    }
}
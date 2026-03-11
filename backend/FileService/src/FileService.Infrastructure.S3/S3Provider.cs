using Amazon.S3;
using Amazon.S3.Model;
using FileService.Contracts;
using FileService.Core;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Result;

namespace FileService.Infrastructure.S3;

public class S3Provider : IS3Provider
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Provider> _logger;
    private readonly S3Options _s3Options;

    private readonly SemaphoreSlim _requestsSemaphore;

    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> s3Options, ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3Options = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(1, _s3Options.MaxConcurrentRequests);
    }

    public async Task<Result> UploadFileAsync(
        StorageKey storageKey,
        Stream stream,
        MediaData mediaData,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Value,
                InputStream = stream,
                ContentType = mediaData.ContentType.Value,
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
            Expires = DateTime.Now.AddMinutes(_s3Options.UploadUrlExpirationMinutes),
            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
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
            Expires = DateTime.Now.AddHours(_s3Options.DownloadExpirationHours),
            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
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

    public async Task<Result<IReadOnlyList<string>>> GenerateDownloadUrlsAsync(
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
                        Expires = DateTime.Now.AddHours(_s3Options.DownloadExpirationHours),
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };
                    string? result = await _s3Client.GetPreSignedURLAsync(request);
                    return result;
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });
            string[] results = await Task.WhenAll(tasks);
            return Result<IReadOnlyList<string>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download urls");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> StartMultiPartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName, Key = key, ContentType = contentType
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

    public async Task<Result<IReadOnlyList<string>>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
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
                            BucketName = bucketName,
                            Key = key,
                            Verb = HttpVerb.PUT,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.AddMinutes(_s3Options.UploadUrlExpirationMinutes),
                            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
                        };
                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return url;
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });

            string[] results = await Task.WhenAll(tasks);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error get chunks url multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> GenerateDownloadUrlAsync(string bucketName, string key)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadExpirationHours),
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
            };
            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> GenerateUploadUrlAsync(string bucketName, string key)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadExpirationHours),
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
            };
            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string>> CompleteMultiPartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartEtagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new PartETag { ETag = p.ETag, PartNumber = p.PartNumber }).ToList(),
            };

            var response = await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return response.Key;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }
}
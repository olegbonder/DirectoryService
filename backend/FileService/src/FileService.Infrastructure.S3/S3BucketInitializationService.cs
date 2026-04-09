using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using FileService.Core.FilesStorage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public class S3BucketInitializationService: BackgroundService
{
    private readonly FileStorageOptions _s3Options;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3BucketInitializationService> _logger;

    public S3BucketInitializationService(
        IOptions<FileStorageOptions> S3Options,
        IAmazonS3 s3Client,
        ILogger<S3BucketInitializationService> logger)
    {
        _s3Options = S3Options.Value;
        _s3Client = s3Client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("S3BucketInitialization service started.");

            if (_s3Options.RequiredBuckets.Count == 0)
            {
                _logger.LogInformation("S3BucketInitialization service required buckets.");
                throw new ArgumentException($"{nameof(_s3Options.RequiredBuckets)} is required ");
            }

            _logger.LogInformation("Starting S3 buckets initialization. Required buckets: {Buckets}", string.Join(", ", _s3Options.RequiredBuckets));

            var tasks = _s3Options.RequiredBuckets
                .Select(bucketName => InitializeBucketAsync(bucketName, stoppingToken))
                .ToArray();
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "S3BucketInitialization operation cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "S3BucketInitialization error.");
        }
    }

    private async Task InitializeBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            var bucketsExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (bucketsExists)
            {
                _logger.LogInformation("Bucket {Bucket} already exists", bucketName);
                return;
            }

            _logger.LogInformation("Creating bucket {Bucket}", bucketName);
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName
            };

            await _s3Client.PutBucketAsync(putBucketRequest, cancellationToken);

            string policy = $$"""
                               {
                                   "Version": "2012-10-17",
                                   "Statement": [
                                       {
                                           "Effect": "Allow",
                                           "Principal": "*",
                                           "Action": ["s3:GetObject"],
                                           "Resource": ["arn:aws:s3:::{{bucketName}}/*"]
                                       }
                                   ]
                               }
                               """;
            var putBucketPolicyRequest = new PutBucketPolicyRequest
            {
                BucketName = bucketName,
                Policy = policy
            };

            await _s3Client.PutBucketPolicyAsync(putBucketPolicyRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize bucket '{BucketName}'", bucketName);
            throw;
        }
    }
}
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using FileService.Contracts.Dtos.MediaAssets.CompleteMultiPartUpload;
using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.IntegrationTests.Infrastructure;
using FileService.VideoProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Features.VideoProcesses;

public class VideoProcessingTests : FileServiceTestsBase
{
    public VideoProcessingTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenValidVideoUploaded_ShouldCompleteProcessingSuccessfully()
    {
        // arrange
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        await using AsyncServiceScope scope = Services.CreateAsyncScope();

        var processingService = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();

        var videoAssetId = await TestData.UploadTestVideoAsync(cancellationToken);

        // act
        var result = await processingService.ProcessVideoAsync(videoAssetId, cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        MediaAsset? mediaAsset = null;
        string? rawKey = null;

        await ExecuteInDb(async db =>
        {
            mediaAsset = await db.MediaAssets
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == videoAssetId, cancellationToken);

            VideoProcess? videoProcess = await db.VideoProcesses
                .AsNoTracking()
                .FirstOrDefaultAsync(vp => vp.Id == videoAssetId, cancellationToken);

            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.READY, mediaAsset.Status);

            Assert.NotNull(mediaAsset.FinalKey);
            Assert.Equal($"hls/{videoAssetId}/master.m3u8", mediaAsset.FinalKey.Value);

            Assert.NotNull(videoProcess);
            Assert.Equal(VideoProcessStatus.SUCCEEDED, videoProcess.Status);

            VideoAsset? videoAsset = mediaAsset as VideoAsset;
            Assert.NotNull(videoAsset);
            Assert.NotNull(videoAsset.RawKey);
            rawKey = videoAsset.RawKey.Value;
        });

        await ExecuteInS3(async s3Client =>
        {
            StorageKey key = mediaAsset?.FinalKey ?? throw new InvalidOperationException("MediaAsset Key is null");
            string prefix = key.Prefix;

            var listRequest = new ListObjectsV2Request
            {
                BucketName = VideoAsset.BUCKET,
                Prefix = prefix,
            };

            ListObjectsV2Response listResponse = await s3Client.ListObjectsV2Async(listRequest, cancellationToken);

            Assert.NotEmpty(listResponse.S3Objects);

            GetObjectMetadataResponse objectData = await s3Client
                .GetObjectMetadataAsync(VideoAsset.BUCKET, key.Value, cancellationToken);
            Assert.NotNull(objectData);

            AmazonS3Exception exception = await Assert.ThrowsAsync<AmazonS3Exception>(
                async () =>
                    await s3Client.GetObjectMetadataAsync(VideoAsset.BUCKET, rawKey, cancellationToken));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        });
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenNotExistMediaAssetId_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var videoAssetId = Guid.NewGuid();

        await using AsyncServiceScope scope = Services.CreateAsyncScope();

        var processingService = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();

        // act
        var result = await processingService.ProcessVideoAsync(videoAssetId, cancellationToken);

        // assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        var errorCodes = result.Errors.Select(e => e.Code).ToList();
        Assert.Contains(errorCodes, e => e.Contains("video_asset.not.found"));
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenOnlyStartMultiPartUpload_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        await using AsyncServiceScope scope = Services.CreateAsyncScope();

        var processingService = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();

        var startUploadResult = await TestData
            .StartMultiPartUploadAsync(cancellationToken);
        var videoAssetId = startUploadResult.Item1.MediaAssetId;

        // act
        var result = await processingService.ProcessVideoAsync(videoAssetId, cancellationToken);

        // assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenAssetInNotVideoAsset_should_failed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        await using AsyncServiceScope scope = Services.CreateAsyncScope();

        var processingService = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();

        var previewAssetId = await TestData.UploadTestVideoAsync(cancellationToken, true);

        // act
        var result = await processingService.ProcessVideoAsync(previewAssetId, cancellationToken);

        // assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        var errorCodes = result.Errors.Select(e => e.Code).ToList();
        Assert.Contains(errorCodes, e => e.Contains("video_asset.not.found"));
    }
}
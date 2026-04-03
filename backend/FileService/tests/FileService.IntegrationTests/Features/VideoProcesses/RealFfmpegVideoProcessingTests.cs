using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.Infrastructure.Postgres;
using FileService.IntegrationTests.Infrastructure;
using FileService.VideoProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Features.VideoProcesses;

[Collection("RealFfmpegTestsCollection")]
public class RealFfmpegVideoProcessingTests : IAsyncLifetime, IClassFixture<RealFfmpegIntegrationTestsWebFactory>
{
    private readonly RealFfmpegIntegrationTestsWebFactory _factory;

    private HttpClient HttpClient { get; }

    private IServiceProvider Services { get; }

    private HttpClient AppHttpClient { get; }

    private TestData TestData { get; }

    public RealFfmpegVideoProcessingTests(RealFfmpegIntegrationTestsWebFactory factory)
    {
        _factory = factory;
        AppHttpClient = _factory.CreateClient();
        HttpClient = new HttpClient();
        Services = _factory.Services;
        TestData = new TestData(AppHttpClient, HttpClient);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    private async Task ExecuteInDb(Func<FileServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        FileServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        await action(dbContext);
    }

    private async Task ExecuteInS3(Func<IAmazonS3, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        IAmazonS3 s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        await action(s3Client);
    }

    [Fact]
    [Trait("Category", "RealFfmpeg")]
    public async Task ProcessVideoAsync_WithRealFfmpeg_ShouldGenerateValidHls()
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
}

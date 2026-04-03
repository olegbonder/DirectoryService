using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.CompleteMultiPartUpload;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.Infrastructure.S3;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.IntegrationTests.Features
{
    public class CompleteMultipartUploadTests : FileServiceTestsBase
    {
        public CompleteMultipartUploadTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task CompleteMultipartUpload_FullCycle_PersistsVideoFile()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            FileInfo fileInfo = TestData.GetFileInfo();
            var startMultiPartUploadRequest = TestData.SetStartMultiPartUploadRequest(fileInfo);
            var startMultipartResult = await TestData.StartMultiPartUpload(
                startMultiPartUploadRequest,
                cancellationToken);

            var startMultiPartUploadResponse = startMultipartResult.Value;
            var mediaAssetId = startMultiPartUploadResponse.MediaAssetId;
            var partEtags = await TestData.UploadChunks(
                fileInfo,
                startMultiPartUploadResponse.ChunkSize,
                startMultiPartUploadResponse.ChunkUploadUrls,
                cancellationToken);

            var request = new CompleteMultiPartUploadRequest(
                mediaAssetId,
                startMultiPartUploadResponse.UploadId,
                partEtags);

            // act
            var result = await TestData.CompleteMultiPartUpload(request, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(
                        m => m.Id == mediaAssetId,
                        cancellationToken);

                VideoProcess? videoProcess = await db.VideoProcesses
                    .Include(videoProcess => videoProcess.Steps)
                    .FirstOrDefaultAsync(
                        vp => vp.Id == mediaAssetId,
                        cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(mediaAsset.Id, result.Value.MediaAssetId);
                Assert.Equal(MediaStatus.UPLOADED, mediaAsset.Status);

                Assert.True(mediaAsset is VideoAsset);
                Assert.NotNull(videoProcess);
                Assert.Equal(Enum.GetNames<StepType>().Length, videoProcess.Steps.Count);

                var objectS3Response = await GetObjectInS3(mediaAsset.UploadKey, cancellationToken);

                Assert.Equal(fileInfo.Length, objectS3Response.ContentLength);
                Assert.Equal(mediaAsset.UploadKey.Value, objectS3Response.Key);
            });
        }

        [Fact]
        public async Task CompleteMultipartUpload_FullCycle_PersistsImageFile()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            bool isImage = true;
            FileInfo fileInfo = TestData.GetFileInfo(isImage);
            var startMultiPartUploadRequest = TestData.SetStartMultiPartUploadRequest(fileInfo, isImage);
            var startMultipartResult = await TestData.StartMultiPartUpload(
                startMultiPartUploadRequest,
                cancellationToken);

            var startMultiPartUploadResponse = startMultipartResult.Value;
            var mediaAssetId = startMultiPartUploadResponse.MediaAssetId;
            var partEtags = await TestData.UploadChunks(
                fileInfo,
                startMultiPartUploadResponse.ChunkSize,
                startMultiPartUploadResponse.ChunkUploadUrls,
                cancellationToken);

            var request = new CompleteMultiPartUploadRequest(
                mediaAssetId,
                startMultiPartUploadResponse.UploadId,
                partEtags);

            // act
            var result = await TestData.CompleteMultiPartUpload(request, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(
                        m => m.Id == mediaAssetId,
                        cancellationToken);

                VideoProcess? videoProcess = await db.VideoProcesses.Include(videoProcess => videoProcess.Steps)
                    .FirstOrDefaultAsync(
                        vp => vp.Id == mediaAssetId,
                        cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(mediaAsset.Id, result.Value.MediaAssetId);
                Assert.Equal(MediaStatus.UPLOADED, mediaAsset.Status);

                Assert.True(mediaAsset is PreviewAsset);
                Assert.Null(videoProcess);

                var objectS3Response = await GetObjectInS3(mediaAsset.UploadKey, cancellationToken);

                Assert.Equal(fileInfo.Length, objectS3Response.ContentLength);
                Assert.Equal(mediaAsset.UploadKey.Value, objectS3Response.Key);
            });
        }

        [Fact]
        public async Task CompleteMultipartUpload_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new CompleteMultiPartUploadRequest(Guid.Empty, string.Empty, []);

            // act
            var result = await TestData.CompleteMultiPartUpload(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(3, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("mediaassetid.is.empty"));
            Assert.Contains(errorCodes, e => e.Contains("uploadid.is.empty"));
            Assert.Contains(errorCodes, e => e.Contains("partETags.count"));
        }

        [Fact]
        public async Task CompleteMultipartUpload_with_not_ExpectedChunks_count_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var optionsS3 = Services.GetRequiredService<IOptions<S3Options>>();

            // Меняем рекомендуемый размер чанка на 1МБ,
            // т.к. размер тестируемого файла 5 МБ,
            // а рекомендуемый размер чанка по-умолчанию 10 МБ
            optionsS3.Value.RecommendedChunkSizeBytes = 1024 * 1024;

            FileInfo fileInfo = TestData.GetFileInfo();
            var startMultiPartUploadRequest = TestData.SetStartMultiPartUploadRequest(fileInfo);
            var startMultipartResult = await TestData.StartMultiPartUpload(startMultiPartUploadRequest, cancellationToken);

            var startMultiPartUploadResponse = startMultipartResult.Value;
            List<ChunkUploadUrl> chunkUploadUrls = [..startMultiPartUploadResponse.ChunkUploadUrls];
            chunkUploadUrls.RemoveAt(startMultiPartUploadResponse.ChunkUploadUrls.Count - 1);

            var partEtags = await TestData.UploadChunks(
                fileInfo,
                startMultiPartUploadResponse.ChunkSize,
                chunkUploadUrls,
                cancellationToken);

            var request = new CompleteMultiPartUploadRequest(
                startMultiPartUploadResponse.MediaAssetId,
                startMultiPartUploadResponse.UploadId,
                partEtags);

            // act
            var result = await TestData.CompleteMultiPartUpload(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("expected.chunks.count"));

            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(
                        m => m.Id == startMultiPartUploadResponse.MediaAssetId,
                        cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.NotEqual(MediaStatus.UPLOADED, mediaAsset.Status);
            });
        }
    }
}
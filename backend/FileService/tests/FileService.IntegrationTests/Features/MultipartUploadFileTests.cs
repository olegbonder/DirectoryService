using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FileService.IntegrationTests.Features
{
    public class MultipartUploadFileTests : FileServiceTestsBase
    {
        public MultipartUploadFileTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task MultipartUpload_with_valid_video_request_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            FileInfo fileInfo = TestData.GetFileInfo();
            var request = TestData.SetStartMultiPartUploadRequest(fileInfo);

            // act
            var startMultipartResult = await TestData.StartMultiPartUpload(request, cancellationToken);

            Assert.True(startMultipartResult.IsSuccess);
            Assert.NotNull(startMultipartResult.Value.UploadId);
            Assert.True(startMultipartResult.Value.ChunkSize > 0);
            Assert.True(startMultipartResult.Value.ChunkUploadUrls.Count > 0);

            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(m => m.Id == startMultipartResult.Value.MediaAssetId, cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(MediaStatus.UPLOADING, mediaAsset.Status);

                Assert.True(mediaAsset is VideoAsset);
            });
        }

        [Fact]
        public async Task MultipartUpload_with_valid_image_request_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            bool isImage = true;
            FileInfo fileInfo = TestData.GetFileInfo(isImage);
            var request = TestData.SetStartMultiPartUploadRequest(fileInfo, isImage);

            // act
            var startMultipartResult = await TestData.StartMultiPartUpload(request, cancellationToken);

            Assert.True(startMultipartResult.IsSuccess);
            Assert.NotNull(startMultipartResult.Value.UploadId);
            Assert.True(startMultipartResult.Value.ChunkSize > 0);
            Assert.True(startMultipartResult.Value.ChunkUploadUrls.Count > 0);

            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(m => m.Id == startMultipartResult.Value.MediaAssetId, cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(MediaStatus.UPLOADING, mediaAsset.Status);

                Assert.True(mediaAsset is PreviewAsset);
            });
        }

        [Fact]
        public async Task MultipartUpload_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new StartMultiPartUploadRequest(
                string.Empty,
                "test",
                "video/mp4",
                0,
                "lesson",
                Guid.Empty);

            // act
            var result = await TestData.StartMultiPartUpload(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(4, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("filename.is.empty"));
            Assert.Contains(errorCodes, e => e.Contains("media_asset.length"));
            Assert.Contains(errorCodes, e => e.Contains("asset_type.failed"));
            Assert.Contains(errorCodes, e => e.Contains("context.failed"));
        }
    }
}
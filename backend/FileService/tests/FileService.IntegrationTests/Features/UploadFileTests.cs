using FileService.Contracts.Dtos.MediaAssets.UploadFile;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FileService.IntegrationTests.Features
{
    public class UploadFileTests : FileServiceTestsBase
    {
        public UploadFileTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task UploadFile_with_valid_request_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var formFile = TestData.GetFormFile();

            var request = TestData.SetUploadFileRequest(formFile);

            // act
            var result = await TestData.UploadFile(request, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            var mediaAssetId = result.Value;
            Assert.NotEqual(Guid.Empty, mediaAssetId);

            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(m => m.Id == mediaAssetId, cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(MediaStatus.UPLOADED, mediaAsset.Status);

                var objectS3Response = await GetObjectInS3(mediaAsset.RawKey, cancellationToken);

                Assert.Equal(formFile.Length, objectS3Response.ContentLength);
                Assert.Equal(mediaAsset.RawKey.Value, objectS3Response.Key);
            });
        }

        [Fact]
        public async Task UploadFile_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            await using var emptyStream = new MemoryStream();
            var emptyFormFile = new FormFile(emptyStream, 0, 0, "file", "empty.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var request = new UploadFileRequest(
                emptyFormFile,
                "test",
                "lesson",
                Guid.Empty);

            // act
            var result = await TestData.UploadFile(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(3, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("media_asset.length"));
            Assert.Contains(errorCodes, e => e.Contains("asset_type.failed"));
            Assert.Contains(errorCodes, e => e.Contains("context.failed"));
        }
    }
}
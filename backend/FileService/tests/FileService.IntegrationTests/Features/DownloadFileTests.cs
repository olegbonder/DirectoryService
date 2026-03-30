using FileService.Contracts.Dtos.MediaAssets.DownloadFile;
using FileService.Contracts.Dtos.MediaAssets.UploadFile;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Framework.HttpCommunication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;

namespace FileService.IntegrationTests.Features
{
    public class DownloadFileTests : FileServiceTestsBase
    {
        public DownloadFileTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task DownloadFile_with_valid_request_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            FileInfo fileInfo = new(Path.Combine(
                AppContext.BaseDirectory,
                Constants.TEST_FILE_DIRECTORY,
                Constants.TEST_FILE_NAME));
            await using var stream = fileInfo.OpenRead();
            var formFile = new FormFile(stream, 0, stream.Length, "file", fileInfo.Name)
            {
                Headers = new HeaderDictionary(),
                ContentType = "video/mp4"
            };

            var request = new UploadFileRequest(
                formFile,
                "video",
                "department",
                Guid.NewGuid());

            var uploadMediaAssetResult = await TestData.UploadFile(request, cancellationToken);
            var uploadMediaAssetId = uploadMediaAssetResult.Value;

            // act
            var result = await DownloadFile(uploadMediaAssetId!.Value, cancellationToken);
            string downloadUrl = result.Value.DownloadUrl;

            // assert
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(downloadUrl);

            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(m => m.Id == uploadMediaAssetId, cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.NotEqual(MediaStatus.DELETED, mediaAsset.Status);

                var objectS3Response = await GetObjectInS3(mediaAsset.RawKey, cancellationToken);

                Assert.Equal(fileInfo.Length, objectS3Response.ContentLength);
                Assert.Equal(mediaAsset.RawKey.Value, objectS3Response.Key);
            });
        }

        [Fact]
        public async Task DownloadFile_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            var mediaAssetId = Guid.NewGuid();

            // act
            var result = await DownloadFile(mediaAssetId, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("media_asset.not.found"));
        }

        private async Task<Result<DownloadFileResponse>> DownloadFile(
            Guid fileId,
            CancellationToken cancellationToken)
        {
            var downloadFileResponse = await AppHttpClient.GetAsync(
                $"{Constants.DOWNLOAD_FILE_URL}{fileId}",
                cancellationToken);

            var downloadFileResult = await downloadFileResponse
                .HandleResponseAsync<DownloadFileResponse>(cancellationToken);

            return downloadFileResult;
        }
    }
}
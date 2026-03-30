using System.Net.Http.Json;
using FileService.Contracts.Dtos.MediaAssets.GetDownloadUrl;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Framework.HttpCommunication;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;

namespace FileService.IntegrationTests.Features
{
    public class GetDownloadUrlTests : FileServiceTestsBase
    {
        public GetDownloadUrlTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetDownloadUrl_with_valid_request_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var formFile = TestData.GetFormFile();

            var uploadFileRequest = TestData.SetUploadFileRequest(formFile);

            var uploadMediaAssetResult = await TestData.UploadFile(uploadFileRequest, cancellationToken);
            var uploadMediaAssetId = uploadMediaAssetResult.Value!.Value;
            var request = new GetDownloadUrlRequest(uploadMediaAssetId);

            // act
            var result = await GetDownloadUrl(request, cancellationToken);
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

                Assert.Equal(formFile.Length, objectS3Response.ContentLength);
                Assert.Equal(mediaAsset.RawKey.Value, objectS3Response.Key);
            });
        }

        [Fact]
        public async Task GetDownloadUrl_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            var mediaAssetId = Guid.NewGuid();
            var request = new GetDownloadUrlRequest(mediaAssetId);

            // act
            var result = await GetDownloadUrl(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("media_asset.not.found"));
        }

        private async Task<Result<GetDownloadUrlResponse>> GetDownloadUrl(
            GetDownloadUrlRequest request,
            CancellationToken cancellationToken)
        {
            var getDownloadUrlResponse = await AppHttpClient.PostAsJsonAsync(
                Constants.GET_DOWNLOAD_URL,
                request,
                cancellationToken);

            var downloadFileResult = await getDownloadUrlResponse
                .HandleResponseAsync<GetDownloadUrlResponse>(cancellationToken);

            return downloadFileResult;
        }
    }
}
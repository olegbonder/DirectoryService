using System.Net.Http.Json;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAssets;
using FileService.IntegrationTests.Infrastructure;
using Framework.HttpCommunication;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;

namespace FileService.IntegrationTests.Features
{
    public class GetMediaAssetsInfoTests : FileServiceTestsBase
    {
        public GetMediaAssetsInfoTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetMediaAssetsInfo_with_valid_request_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            FileInfo fileInfo = TestData.GetFileInfo();
            var startMultipartRequest = TestData.SetStartMultiPartUploadRequest(fileInfo);

            var firstStartMultipartResult = await TestData.StartMultiPartUpload(startMultipartRequest, cancellationToken);
            var secondStartMultipartResult = await TestData.StartMultiPartUpload(startMultipartRequest, cancellationToken);
            var firstMediaAssetId = firstStartMultipartResult.Value.MediaAssetId;
            var secondMediaAssetId = secondStartMultipartResult.Value.MediaAssetId;
            var mediaAssetIds = new List<Guid> { firstMediaAssetId, secondMediaAssetId };
            var request = new GetMediaAssetsRequest(mediaAssetIds);

            // act
            var result = await GetMediaAssetsInfo(request, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            var mediaAssetInfo = result.Value;
            Assert.NotNull(mediaAssetInfo);
            var mediaAssetInfoItems = mediaAssetInfo.Items.ToList();
            Assert.Equal(2, mediaAssetInfoItems.Count());

            await ExecuteInDb(async db =>
            {
                var mediaAssets = await db.MediaAssets
                    .Where(m => mediaAssetIds.Contains(m.Id)).ToListAsync(cancellationToken);

                Assert.Equivalent(
                    mediaAssets.Select(m => m.Id),
                    mediaAssetInfoItems.Select(m => m.Id));
                Assert.Equivalent(
                    mediaAssets.Select(m => m.Status.ToString().ToLowerInvariant()),
                    mediaAssetInfoItems.Select(m => m.Status.ToString().ToLowerInvariant()));
                Assert.All(mediaAssetInfoItems, m => Assert.Null(m.DownloadUrl));
            });
        }

        [Fact]
        public async Task GetMediaAssetsInfo_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new GetMediaAssetsRequest([]);

            // act
            var result = await GetMediaAssetsInfo(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("mediaAssetIds.not.empty"));
        }

        [Fact]
        public async Task GetMediaAssetsInfo_with_not_exist_media_asset_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            var mediaAssetId = Guid.NewGuid();
            var request = new GetMediaAssetsRequest([mediaAssetId]);

            // act
            var result = await GetMediaAssetsInfo(request, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.Items);
        }

        private async Task<Result<GetMediaAssetsResponse>> GetMediaAssetsInfo(
            GetMediaAssetsRequest request,
            CancellationToken cancellationToken)
        {
            var mediaAssetsInfoResponse = await AppHttpClient.PostAsJsonAsync(
                Constants.GET_MEDIA_ASSETS_INFO_URL,
                request,
                cancellationToken);

            var mediaAssetsInfoResult = await mediaAssetsInfoResponse
                .HandleResponseAsync<GetMediaAssetsResponse>(cancellationToken);

            return mediaAssetsInfoResult;
        }
    }
}
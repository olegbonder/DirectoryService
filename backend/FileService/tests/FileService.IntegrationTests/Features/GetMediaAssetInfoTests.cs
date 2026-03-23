using FileService.Contracts.MediaAssets.GetMediaAsset;
using FileService.Contracts.MediaAssets.StartMultiPartUpload;
using FileService.Core.HttpCommunication;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;

namespace FileService.IntegrationTests.Features
{
    public class GetMediaAssetInfoTests : FileServiceTestsBase
    {
        public GetMediaAssetInfoTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetMediaAssetInfo_with_valid_request_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            FileInfo fileInfo = new(Path.Combine(
                AppContext.BaseDirectory,
                Constants.TEST_FILE_DIRECTORY,
                Constants.TEST_FILE_NAME));
            var startMultipartRequest = new StartMultiPartUploadRequest(
                fileInfo.Name,
                "video",
                "video/mp4",
                fileInfo.Length,
                "department",
                Guid.NewGuid());

            var startMultipartResult = await TestData.StartMultiPartUpload(startMultipartRequest, cancellationToken);
            var startMultipartResponse = startMultipartResult.Value;
            var mediaAssetId = startMultipartResponse.MediaAssetId;

            // act
            var result = await GetMediaAssetInfo(mediaAssetId, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            var mediaAssetInfo = result.Value;
            Assert.NotNull(mediaAssetInfo);

            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(m => m.Id == mediaAssetId, cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(mediaAsset.Id, mediaAssetInfo.Id);
                Assert.Equal(mediaAsset.Status.ToString().ToLowerInvariant(), mediaAssetInfo.Status);
                Assert.Equal(mediaAsset.AssetType.ToString().ToLowerInvariant(), mediaAssetInfo.AssetType);
                Assert.Equal(mediaAsset.CreatedAt, mediaAssetInfo.CreatedAt);
                Assert.Equal(mediaAsset.UpdatedAt, mediaAssetInfo.UpdatedAt);
                Assert.Equal(mediaAsset.MediaData.FileName.Value, mediaAssetInfo.FileInfo.FileName);
                Assert.Equal(mediaAsset.MediaData.ContentType.Value, mediaAssetInfo.FileInfo.ContentType);
                Assert.Equal(mediaAsset.MediaData.Size, mediaAssetInfo.FileInfo.Size);
                Assert.Null(mediaAssetInfo.DownloadUrl);
            });
        }

        [Fact]
        public async Task GetMediaAssetInfo_with_not_exist_media_asset_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            var mediaAssetId = Guid.NewGuid();

            // act
            var result = await GetMediaAssetInfo(mediaAssetId, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        private async Task<Result<GetMediaAssetDto?>> GetMediaAssetInfo(
            Guid mediaAssetId,
            CancellationToken cancellationToken)
        {
            var mediaAssetInfoResponse = await AppHttpClient.GetAsync(
                $"{Constants.GET_MEDIA_ASSET_INFO_URL}{mediaAssetId}",
                cancellationToken);

            var mediaAssetInfoResult = await mediaAssetInfoResponse
                .HandleResponseAsync<GetMediaAssetDto?>(cancellationToken);

            return mediaAssetInfoResult;
        }
    }
}
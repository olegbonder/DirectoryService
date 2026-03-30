using Amazon.S3.Model;
using FileService.Contracts.Dtos.MediaAssets;
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
    public class DeleteFileTests : FileServiceTestsBase
    {
        public DeleteFileTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task DeleteFile_with_valid_request_should_success()
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
            var uploadMediaAssetId = uploadMediaAssetResult.Value.Value;

            // act
            var result = await DeleteFile(uploadMediaAssetId, cancellationToken);

            // assert
            var deleteMediaAssetId = result.Value.MediaAssetId;
            Assert.True(result.IsSuccess);
            Assert.Equal(uploadMediaAssetId, deleteMediaAssetId);

            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(m => m.Id == uploadMediaAssetId, cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(MediaStatus.DELETED, mediaAsset.Status);

                var exception = await Assert.ThrowsAsync<NoSuchKeyException>(
                    async () => await GetObjectInS3(mediaAsset.RawKey, cancellationToken));

                Assert.IsType<NoSuchKeyException>(exception);
            });
        }

        [Fact]
        public async Task DeleteFile_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            var mediaAssetId = Guid.NewGuid();

            // act
            var result = await DeleteFile(mediaAssetId, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("media_asset.not.found"));
        }

        private async Task<Result<MediaAssetResponse>> DeleteFile(
            Guid deleteFileId,
            CancellationToken cancellationToken)
        {
            var deleteFileResponse = await AppHttpClient.DeleteAsync(
                $"{Constants.DELETE_FILE_URL}{deleteFileId}",
                cancellationToken);

            var deleteFileResult = await deleteFileResponse
                .HandleResponseAsync<MediaAssetResponse>(cancellationToken);

            return deleteFileResult;
        }
    }
}
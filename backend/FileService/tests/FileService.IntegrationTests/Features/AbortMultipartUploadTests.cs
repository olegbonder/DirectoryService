using System.Net.Http.Json;
using Amazon.S3;
using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Framework.HttpCommunication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Result;
using AbortMultipartUploadRequest = FileService.Contracts.Dtos.MediaAssets.AbortMultipartUpload.AbortMultipartUploadRequest;

namespace FileService.IntegrationTests.Features
{
    public class AbortMultipartUploadTests : FileServiceTestsBase
    {
        public AbortMultipartUploadTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task AbortMultipartUpload_with_valid_request_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            int partNumber = 1;

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
            string uploadId = startMultipartResponse.UploadId;

            var request = new AbortMultipartUploadRequest(mediaAssetId, uploadId);

            // act
            var result = await AbortMultipartUpload(request, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);

            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(m => m.Id == mediaAssetId, cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(MediaStatus.FAILED, mediaAsset.Status);

                var amazonS3Client = Services.GetRequiredService<IAmazonS3>();

                var exception = await Assert.ThrowsAsync<AmazonS3Exception>(
                     async () => await amazonS3Client.ListPartsAsync(
                         mediaAsset.RawKey.Bucket,
                         mediaAsset.RawKey.Value,
                         uploadId,
                         cancellationToken));

                Assert.IsType<AmazonS3Exception>(exception);
            });
        }

        [Fact]
        public async Task AbortMultipartUpload_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new AbortMultipartUploadRequest(Guid.Empty, string.Empty);

            // act
            var result = await AbortMultipartUpload(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(2, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("mediaassetid.is.empty"));
            Assert.Contains(errorCodes, e => e.Contains("uploadid.is.empty"));
        }

        private async Task<Result> AbortMultipartUpload(
            AbortMultipartUploadRequest request,
            CancellationToken cancellationToken)
        {
            var abortMultipartUploadResponse = await AppHttpClient.PostAsJsonAsync(
                Constants.ABORT_MULTIPART_UPLOAD_URL,
                request,
                cancellationToken);

            var chunkUploadUrlResult = await abortMultipartUploadResponse
                .HandleResponseAsync(cancellationToken);

            return chunkUploadUrlResult;
        }
    }
}
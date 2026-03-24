using System.Net.Http.Json;
using Amazon.S3.Model;
using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.GetChunkUploadUrl;
using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;
using FileService.Core.HttpCommunication;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;

namespace FileService.IntegrationTests.Features
{
    public class GetChunkUploadUrlTests : FileServiceTestsBase
    {
        public GetChunkUploadUrlTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetChunkUploadUrl_with_valid_request_should_success()
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
            string chunkUploadUrl = startMultipartResponse.ChunkUploadUrls
                .First(c => c.PartNumber == partNumber).UploadUrl;

            // Должна сформироваться другая ссылка
            Thread.Sleep(1000);
            var request = new GetChunkUploadUrlRequest(mediaAssetId, uploadId, partNumber);

            // act
            var result = await GetChunkUploadUrl(request, cancellationToken);

            // assert
            var getChunkUploadUrl = result.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(partNumber, getChunkUploadUrl.PartNumber);
            Assert.NotEqual(chunkUploadUrl, getChunkUploadUrl.UploadUrl);

            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(m => m.Id == mediaAssetId, cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(MediaStatus.UPLOADING, mediaAsset.Status);

                var uploadChunkResponse = await UploadChunk(
                    fileInfo,
                    startMultipartResponse.ChunkSize,
                    getChunkUploadUrl,
                    cancellationToken);
                Assert.True(uploadChunkResponse.IsSuccessStatusCode);
                Assert.NotEmpty(uploadChunkResponse.Headers.ETag?.Tag.Trim('"'));

                var exception = await Assert.ThrowsAsync<NoSuchKeyException>(
                    async () => await GetObjectInS3(mediaAsset.RawKey, cancellationToken));

                Assert.IsType<NoSuchKeyException>(exception);
            });
        }

        [Fact]
        public async Task GetChunkUploadUrl_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new GetChunkUploadUrlRequest(Guid.Empty, string.Empty, 0);

            // act
            var result = await GetChunkUploadUrl(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(3, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("mediaassetid.is.empty"));
            Assert.Contains(errorCodes, e => e.Contains("uploadid.is.empty"));
            Assert.Contains(errorCodes, e => e.Contains("partNumber"));
        }

        [Fact]
        public async Task GetChunkUploadUrl_with_not_exist_partNumber_should_failed()
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
            string uploadId = startMultipartResponse.UploadId;
            int partNumber = startMultipartResponse.ChunkUploadUrls.Max(c => c.PartNumber) + 1;

            var request = new GetChunkUploadUrlRequest(mediaAssetId, uploadId, partNumber);

            // act
            var result = await GetChunkUploadUrl(request, cancellationToken);

            // assert
            var getChunkUploadUrl = result.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(partNumber, getChunkUploadUrl.PartNumber);
            /*Assert.NotEmpty(result.Errors);
            Assert.Equal(3, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("partNumber"));*/
        }

        private async Task<Result<ChunkUploadUrl>> GetChunkUploadUrl(
            GetChunkUploadUrlRequest request,
            CancellationToken cancellationToken)
        {
            var chunkUploadUrlResponse = await AppHttpClient.PostAsJsonAsync(
                Constants.GET_CHUNK_UPLOAD_URL,
                request,
                cancellationToken);

            var chunkUploadUrlResult = await chunkUploadUrlResponse
                .HandleResponseAsync<ChunkUploadUrl>(cancellationToken);

            return chunkUploadUrlResult;
        }

        private async Task<HttpResponseMessage> UploadChunk(
            FileInfo fileInfo,
            int chunkSize,
            ChunkUploadUrl chunkUploadUrl,
            CancellationToken cancellationToken)
        {
            await using var stream = fileInfo.OpenRead();

            byte[] chunk = new byte[chunkSize];
            int bytesRead = await stream.ReadAsync(chunk.AsMemory(0, chunkSize), cancellationToken);
            if (bytesRead == 0)
                throw new Exception("can't read chunk");

            var content = new ByteArrayContent(chunk);
            var response = await HttpClient.PutAsync(chunkUploadUrl.UploadUrl, content, cancellationToken);

            return response;
        }
    }
}
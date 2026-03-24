using System.Net.Http.Json;
using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.CompleteMultiPartUpload;
using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;
using FileService.Core.HttpCommunication;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Infrastructure.S3;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedKernel.Result;
using Xunit.Abstractions;

namespace FileService.IntegrationTests.Features
{
    public class CompleteMultipartUploadTests : FileServiceTestsBase
    {
        private readonly ITestOutputHelper _output;

        public CompleteMultipartUploadTests(IntegrationTestsWebFactory factory, ITestOutputHelper output)
            : base(factory)
        {
            _output = output;
        }

        [Fact]
        public async Task CompleteMultipartUpload_FullCycle_PersistsMediaFile()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;

            FileInfo fileInfo = new(Path.Combine(
                AppContext.BaseDirectory,
                Constants.TEST_FILE_DIRECTORY,
                Constants.TEST_FILE_NAME));
            var startMultiPartUploadRequest = new StartMultiPartUploadRequest(
                fileInfo.Name,
                "video",
                "video/mp4",
                fileInfo.Length,
                "department",
                Guid.NewGuid());
            var startMultipartResult = await TestData.StartMultiPartUpload(startMultiPartUploadRequest, cancellationToken);

            var startMultiPartUploadResponse = startMultipartResult.Value;
            var partEtags = await UploadChunks(
                fileInfo,
                startMultiPartUploadResponse.ChunkSize,
                startMultiPartUploadResponse.ChunkUploadUrls,
                cancellationToken);

            var request = new CompleteMultiPartUploadRequest(
                startMultiPartUploadResponse.MediaAssetId,
                startMultiPartUploadResponse.UploadId,
                partEtags);

            // act
            var result = await CompleteMultiPartUpload(request, cancellationToken);

            // assert
            
            _output.WriteLine($"Value: {result.Value}");
            _output.WriteLine($"Errors: {string.Join(";", result.Errors?.Select(e => new {e.Message, e.Code}))}");
            Assert.True(result.IsSuccess);
            await ExecuteInDb(async db =>
            {
                MediaAsset? mediaAsset = await db.MediaAssets
                    .FirstOrDefaultAsync(m => m.Id == startMultiPartUploadResponse.MediaAssetId, cancellationToken);

                Assert.NotNull(mediaAsset);
                Assert.Equal(mediaAsset.Id, result.Value.MediaAssetId);
                Assert.Equal(MediaStatus.UPLOADED, mediaAsset.Status);

                var objectS3Response = await GetObjectInS3(mediaAsset.RawKey, cancellationToken);

                Assert.Equal(fileInfo.Length, objectS3Response.ContentLength);
                Assert.Equal(mediaAsset.RawKey.Value, objectS3Response.Key);
            });
        }

        [Fact]
        public async Task CompleteMultipartUpload_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var request = new CompleteMultiPartUploadRequest(Guid.Empty, string.Empty, []);

            // act
            var result = await CompleteMultiPartUpload(request, cancellationToken);

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

            FileInfo fileInfo = new(Path.Combine(
                AppContext.BaseDirectory,
                Constants.TEST_FILE_DIRECTORY,
                Constants.TEST_FILE_NAME));
            var startMultiPartUploadRequest = new StartMultiPartUploadRequest(
                fileInfo.Name,
                "video",
                "video/mp4",
                fileInfo.Length,
                "department",
                Guid.NewGuid());
            var startMultipartResult = await TestData.StartMultiPartUpload(startMultiPartUploadRequest, cancellationToken);

            var startMultiPartUploadResponse = startMultipartResult.Value;
            List<ChunkUploadUrl> chunkUploadUrls = [..startMultiPartUploadResponse.ChunkUploadUrls];
            chunkUploadUrls.RemoveAt(startMultiPartUploadResponse.ChunkUploadUrls.Count - 1);

            var partEtags = await UploadChunks(
                fileInfo,
                startMultiPartUploadResponse.ChunkSize,
                chunkUploadUrls,
                cancellationToken);

            var request = new CompleteMultiPartUploadRequest(
                startMultiPartUploadResponse.MediaAssetId,
                startMultiPartUploadResponse.UploadId,
                partEtags);

            // act
            var result = await CompleteMultiPartUpload(request, cancellationToken);

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

        private async Task<IReadOnlyList<PartEtagDto>> UploadChunks(
            FileInfo fileInfo,
            int chunkSize,
            IReadOnlyList<ChunkUploadUrl> chunkUploadUrls,
            CancellationToken cancellationToken)
        {
            await using var stream = fileInfo.OpenRead();

            var parts = new List<PartEtagDto>();

            foreach (ChunkUploadUrl chunkUploadUrl in chunkUploadUrls.OrderBy(c => c.PartNumber))
            {
                byte[] chunk = new byte[chunkSize];
                int bytesRead = await stream.ReadAsync(chunk.AsMemory(0, chunkSize), cancellationToken);
                if (bytesRead == 0)
                    break;

                var content = new ByteArrayContent(chunk);
                var response = await HttpClient.PutAsync(chunkUploadUrl.UploadUrl, content, cancellationToken);

                string? etag = response.Headers.ETag?.Tag.Trim('"');

                parts.Add(new PartEtagDto(chunkUploadUrl.PartNumber, etag!));
            }

            return parts;
        }

        private async Task<Result<MediaAssetResponse>> CompleteMultiPartUpload(
            CompleteMultiPartUploadRequest request,
            CancellationToken cancellationToken)
        {
            var completeResponse = await AppHttpClient.PostAsJsonAsync(
                Constants.COMPLETE_MULTIPART_UPLOAD_URL,
                request,
                cancellationToken);
            var completeResult = await completeResponse.HandleResponseAsync<MediaAssetResponse>(cancellationToken);

            return completeResult;
        }
    }
}
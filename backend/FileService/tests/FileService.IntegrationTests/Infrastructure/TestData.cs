using System.Net.Http.Headers;
using System.Net.Http.Json;
using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.CompleteMultiPartUpload;
using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;
using FileService.Contracts.Dtos.MediaAssets.UploadFile;
using Framework.HttpCommunication;
using Microsoft.AspNetCore.Http;
using SharedKernel.Result;

namespace FileService.IntegrationTests.Infrastructure
{
    public class TestData
    {
        private readonly HttpClient _appHttpClient;
        private readonly HttpClient _httpClient;

        public TestData(
            HttpClient appHttpClient,
            HttpClient httpClient)
        {
            _appHttpClient = appHttpClient;
            _httpClient = httpClient;
        }

        public FileInfo GetFileInfo(bool isImage = false)
        {
            FileInfo fileInfo = new(Path.Combine(
                AppContext.BaseDirectory,
                Constants.TEST_FILE_DIRECTORY,
                isImage ? Constants.TEST_IMAGE_FILE_NAME : Constants.TEST_VIDEO_FILE_NAME));

            return fileInfo;
        }

        public FormFile GetFormFile(bool isImage = false)
        {
            var fileInfo = GetFileInfo(isImage);
            var stream = fileInfo.OpenRead();
            var formFile = new FormFile(stream, 0, stream.Length, "file", fileInfo.Name)
            {
                Headers = new HeaderDictionary(),
                ContentType = isImage ? "image/jpeg" : "video/mp4"
            };

            return formFile;
        }

        public UploadFileRequest SetUploadFileRequest(FormFile formFile, bool isImage = false)
        {
            var request = new UploadFileRequest(
                formFile,
                isImage ? "preview" : "video",
                "department",
                Guid.NewGuid());

            return request;
        }

        public async Task<Result<Guid?>> UploadFile(
            UploadFileRequest request,
            CancellationToken cancellationToken)
        {
            var fileContent = new StreamContent(request.File.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.File.ContentType);
            var content = new MultipartFormDataContent
            {
                { fileContent, "file", request.File.FileName },
                { new StringContent(request.AssetType), "assetType" },
                { new StringContent(request.Context), "context" },
                { new StringContent(request.ContextId.ToString()), "contextId" }
            };

            var uploadFileResponse = await _appHttpClient.PostAsync(
                Constants.UPLOAD_FILE_URL,
                content,
                cancellationToken);

            var uploadFileResult = await uploadFileResponse
                .HandleNullableResponseAsync<Guid?>(cancellationToken);

            return uploadFileResult;
        }

        public StartMultiPartUploadRequest SetStartMultiPartUploadRequest(FileInfo fileInfo, bool isImage = false)
        {
            var startMultiPartUploadRequest = new StartMultiPartUploadRequest(
                fileInfo.Name,
                isImage ? "preview" : "video",
                isImage ? "image/jpeg" : "video/mp4",
                fileInfo.Length,
                "department",
                Guid.NewGuid());

            return startMultiPartUploadRequest;
        }

        public async Task<Result<StartMultiPartUploadResponse>> StartMultiPartUpload(
            StartMultiPartUploadRequest request,
            CancellationToken cancellationToken)
        {
            var startMultipartResponse = await _appHttpClient.PostAsJsonAsync(
                Constants.START_MULTIPART_UPLOAD_URL,
                request,
                cancellationToken);

            var startMultipartResult = await startMultipartResponse
                .HandleResponseAsync<StartMultiPartUploadResponse>(cancellationToken);

            return startMultipartResult;
        }

        public async Task<IReadOnlyList<PartEtagDto>> UploadChunks(
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
                var response = await _httpClient.PutAsync(chunkUploadUrl.UploadUrl, content, cancellationToken);

                string? etag = response.Headers.ETag?.Tag.Trim('"');

                parts.Add(new PartEtagDto(chunkUploadUrl.PartNumber, etag!));
            }

            return parts;
        }

        public async Task<Result<MediaAssetResponse>> CompleteMultiPartUpload(
            CompleteMultiPartUploadRequest request,
            CancellationToken cancellationToken)
        {
            var completeResponse = await _appHttpClient.PostAsJsonAsync(
                Constants.COMPLETE_MULTIPART_UPLOAD_URL,
                request,
                cancellationToken);
            var completeResult = await completeResponse.HandleResponseAsync<MediaAssetResponse>(cancellationToken);

            return completeResult;
        }

        public async Task<(StartMultiPartUploadResponse, FileInfo)> StartMultiPartUploadAsync(
            CancellationToken cancellationToken,
            bool isImage = false)
        {
            FileInfo fileInfo = GetFileInfo(isImage);

            var startRequest = SetStartMultiPartUploadRequest(fileInfo, isImage);
            var startResponse =
                await StartMultiPartUpload(startRequest, cancellationToken);
            var startResult = startResponse.Value;

            return (startResult, fileInfo);
        }

        public async Task<Guid> UploadTestVideoAsync(
            CancellationToken cancellationToken,
            bool isImage = false)
        {
            var startResult =
                await StartMultiPartUploadAsync(cancellationToken, isImage);
            var mediaAssetId = startResult.Item1.MediaAssetId;

            var partEtags = await UploadChunks(
                startResult.Item2,
                startResult.Item1.ChunkSize,
                startResult.Item1.ChunkUploadUrls,
                cancellationToken);

            var completeRequest = new CompleteMultiPartUploadRequest(
                mediaAssetId,
                startResult.Item1.UploadId,
                partEtags);
            await CompleteMultiPartUpload(completeRequest, cancellationToken);

            return mediaAssetId;
        }
    }
}

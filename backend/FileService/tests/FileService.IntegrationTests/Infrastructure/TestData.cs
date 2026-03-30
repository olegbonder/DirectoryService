using System.Net.Http.Headers;
using System.Net.Http.Json;
using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;
using FileService.Contracts.Dtos.MediaAssets.UploadFile;
using Framework.HttpCommunication;
using Microsoft.AspNetCore.Http;
using SharedKernel.Result;

namespace FileService.IntegrationTests.Infrastructure
{
    public class TestData
    {
        private readonly IServiceProvider _services;
        private readonly HttpClient _appHttpClient;

        public TestData(IServiceProvider services, HttpClient appHttpClient)
        {
            _services = services;
            _appHttpClient = appHttpClient;
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

        public UploadFileRequest SetUploadFileRequest(FormFile formFile, bool isImage = false)
        {
            var request = new UploadFileRequest(
                formFile,
                isImage ? "preview" : "video",
                "department",
                Guid.NewGuid());

            return request;
        }
    }
}

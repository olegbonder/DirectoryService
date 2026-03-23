using System.Net.Http.Headers;
using System.Net.Http.Json;
using FileService.Contracts.MediaAssets.StartMultiPartUpload;
using FileService.Contracts.MediaAssets.UploadFile;
using FileService.Core.HttpCommunication;
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

        public async Task<Result<Guid>> UploadFile(
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
                .HandleResponseAsync<Guid>(cancellationToken);

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
    }
}

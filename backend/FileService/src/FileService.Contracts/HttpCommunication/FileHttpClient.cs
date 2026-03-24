using System.Net.Http.Json;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAsset;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAssets;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Contracts.HttpCommunication;

internal sealed class FileHttpClient : IFileCommunicationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileHttpClient> _logger;

    public FileHttpClient(HttpClient httpClient, ILogger<FileHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<GetMediaAssetResponse>> GetMediaAssetInfo(
        Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/files/{mediaAssetId}", cancellationToken);
            return await response.HandleResponseAsync<GetMediaAssetResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media asset for {MediaAssetId}",  mediaAssetId);
            return Error.Failure("server.error", "Failed to get media asset");
        }
    }

    public async Task<Result<GetMediaAssetsResponse>> GetMediaAssetsInfo(
        GetMediaAssetsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/files/batch", request, cancellationToken);
            return await response.HandleResponseAsync<GetMediaAssetsResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media assets for {MediaAssetIds}",  request.MediaAssetIds);
            return Error.Failure("server.error", "Failed to get media assets");
        }
    }
}
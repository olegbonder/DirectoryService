using FileService.Contracts.Dtos.MediaAssets.DownloadFile;
using FileService.Contracts.Dtos.VideoProcessing;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.VideoProcessing;

public sealed class GetVideoProcessingProgressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet(
            "/files/video-processing/progress/{videoAssetId:guid}",
            async Task<EndpointResult<ProgressEventDto>>(
                [FromRoute] Guid videoAssetId,
                [FromServices] GetVideoProcessingProgressHandler handler) =>
        {
            var query = new ProgressEventRequest(videoAssetId);
            return await handler.Handle(query, CancellationToken.None);
        });
    }
}

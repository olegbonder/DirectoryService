using FileService.Contracts.Dtos.MediaAssets.GetMediaAsset;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.GetMediaAssetInfo;

public class GetMediaAssetInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/files/{mediaAssetId:guid}", async Task<EndpointResult<GetMediaAssetResponse?>>(
            Guid mediaAssetId,
            [FromServices] GetMediaAssetInfoHandler handler,
            CancellationToken cancellationToken) =>
        {
            var request = new GetMediaAssetInfoRequest(mediaAssetId);
            return await handler.Handle(request, cancellationToken);
        });
    }
}
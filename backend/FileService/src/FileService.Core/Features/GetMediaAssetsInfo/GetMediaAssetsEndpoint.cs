using FileService.Contracts.Dtos.MediaAssets.GetMediaAssets;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.GetMediaAssetsInfo
{
    public class GetMediaAssetsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("/files/batch", async Task<EndpointResult<GetMediaAssetsResponse>>(
                    [FromBody] GetMediaAssetsRequest request,
                    [FromServices] GetMediaAssetsHandler handler,
                    CancellationToken cancellationToken) =>
                await handler.Handle(request, cancellationToken));
        }
    }
}
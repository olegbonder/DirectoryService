using FileService.Contracts.Dtos.MediaAssets.CheckMediaAssetExists;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.CheckMediaAssetExists
{
    public class CheckMediaAssetExistsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/files/{mediaAssetId:guid}/exists",
                async Task<EndpointResult<CheckMediaAssetExistsResponse>>(
                    [FromRoute] Guid mediaAssetId,
                    [FromServices] CheckMediaAssetExistsHandler handler,
                    CancellationToken cancellationToken) =>
                await handler.Handle(mediaAssetId, cancellationToken));
        }
    }
}
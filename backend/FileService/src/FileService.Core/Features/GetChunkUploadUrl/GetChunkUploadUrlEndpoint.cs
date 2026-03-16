using FileService.Contracts.MediaAssets;
using FileService.Contracts.MediaAssets.GetChunkUploadUrl;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.GetChunkUploadUrl;

public sealed class GetChunkUploadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost(
            "/files/multipart/url",
            async Task<EndpointResult<ChunkUploadUrl>>(
                [FromBody] GetChunkUploadUrlRequest request,
                [FromServices] GetChunkUploadUrlHandler handler,
                CancellationToken cancellationToken) =>
                {
                    var command = new GetChunkUploadUrlCommand(request);
                    return await handler.Handle(command, cancellationToken);
                });
    }
}

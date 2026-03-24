using FileService.Contracts.Dtos.MediaAssets.AbortMultipartUpload;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.AbortMultipartUpload;

public sealed class AbortMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/files/multipart/cancel", async Task<EndpointResult>(
                [FromBody] AbortMultipartUploadRequest request,
                [FromServices] AbortMultipartUploadHandler handler,
                CancellationToken cancellationToken) =>
        {
            var command = new AbortMultipartUploadCommand(request);
            return await handler.Handle(command, cancellationToken);
        });
    }
}

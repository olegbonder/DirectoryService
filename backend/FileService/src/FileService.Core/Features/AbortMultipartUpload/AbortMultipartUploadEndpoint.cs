using FileService.Contracts.MediaAssets.AbortMultipartUpload;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.AbortMultipartUpload;

public sealed class AbortMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/files/multipart/abort", async Task(
                [FromBody] AbortMultipartUploadRequest request,
                [FromServices] AbortMultipartUploadHandler handler,
                CancellationToken cancellationToken) =>
        {
            var command = new AbortMultipartUploadCommand(request);
            await handler.Handle(command, cancellationToken);
        });
    }
}

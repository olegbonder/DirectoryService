using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.StartMultiPartUpload;

public sealed class StartMultiPartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/files/multipart/start", async Task<EndpointResult<StartMultiPartUploadResponse>>(
                [FromBody] StartMultiPartUploadRequest request,
                [FromServices] StartMultiPartUploadHandler handler,
                CancellationToken cancellationToken) =>
        {
            var command = new StartMultiPartUploadCommand(request);
            return await handler.Handle(command, cancellationToken);
        });
    }
}

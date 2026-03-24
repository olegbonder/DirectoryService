using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.CompleteMultiPartUpload;
using FileService.Core.Features.CompleteMultiPartUpload;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.CompleteMultipartUpload;

public sealed class CompleteMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/files/multipart/end", async Task<EndpointResult<MediaAssetResponse>>(
                [FromBody] CompleteMultiPartUploadRequest request,
                [FromServices] CompleteMultiPartUploadHandler handler,
                CancellationToken cancellationToken) =>
        {
            var command = new CompleteMultipartUploadCommand(request);
            return await handler.Handle(command, cancellationToken);
        });
    }
}

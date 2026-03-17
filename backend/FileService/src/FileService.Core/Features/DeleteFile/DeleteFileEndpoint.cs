using FileService.Contracts.MediaAssets;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.DeleteFile;

public class DeleteFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/files/{mediaAssetId:guid}", async Task<EndpointResult<MediaAssetResponse>>(
                [FromRoute] Guid mediaAssetId,
                [FromServices] DeleteFileHandler handler,
                CancellationToken cancellationToken) =>
        {
            var command = new DeleteFileCommand(mediaAssetId);
            return await handler.Handle(command, cancellationToken);
        });
    }
}
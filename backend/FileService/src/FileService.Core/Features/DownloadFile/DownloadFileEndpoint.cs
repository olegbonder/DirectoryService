using FileService.Contracts.MediaAssets.DownloadFile;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.DownloadFile;

public class DownloadFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{id:guid}", async Task<EndpointResult<string>>(
                [FromRoute] Guid id,
                [FromServices] DownloadFileHandler handler,
                CancellationToken cancellationToken) =>
        {
            var request = new DownloadFileRequest(id);
            return await handler.Handle(request, cancellationToken);
        });
    }
}
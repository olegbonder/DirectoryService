using FileService.Contracts.MediaAssets.UploadFile;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.UploadFile;

public class UploadFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/file/upload", async Task<EndpointResult<Guid>>(
                [FromForm] UploadFileRequest request,
                [FromServices] UploadFileHandler handler,
                CancellationToken cancellationToken) =>
        {
            var command = new UploadFileCommand(request);
            return await handler.Handle(command, cancellationToken);
        }).DisableAntiforgery();
    }
}
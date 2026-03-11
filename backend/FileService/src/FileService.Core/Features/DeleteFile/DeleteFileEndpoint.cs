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
        app.MapDelete("/files/{id:guid}", async Task<EndpointResult<Guid>>(
                [FromRoute] Guid id,
                [FromServices] DeleteFileHandler handler,
                CancellationToken cancellationToken) =>
        {
            var command = new DeleteFileCommand(id);
            return await handler.Handle(command, cancellationToken);
        });
    }
}
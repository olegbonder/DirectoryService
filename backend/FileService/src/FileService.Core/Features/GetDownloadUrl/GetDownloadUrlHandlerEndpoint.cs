using FileService.Contracts.MediaAssets.GetDownloadUrl;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.GetDownloadUrl;

public sealed class GetDownloadUrlHandlerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost(
            "/files/url",
            async Task<EndpointResult<GetDownloadUrlResponse>>(
                [FromBody] GetDownloadUrlRequest request,
                [FromServices] GetDownloadUrlHandler handler,
                CancellationToken cancellationToken) =>
                {
                    var command = new GetDownloadUrlCommand(request);
                    return await handler.Handle(command, cancellationToken);
                });
    }
}

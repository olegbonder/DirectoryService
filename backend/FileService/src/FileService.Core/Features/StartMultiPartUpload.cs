using FileService.Contracts;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FileService.Core.Features;

public sealed class StartMultiPartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart-upload", async (
                [FromBody] StartMultiPartUploadRequest request,
                [FromServices] StartMultiPartUploadHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(request, cancellationToken));
    }
}

public sealed class StartMultiPartUploadHandler
{
    private readonly ILogger<StartMultiPartUploadHandler> _logger;
    private readonly IS3Provider _s3Provider;

    public StartMultiPartUploadHandler(
        ILogger<StartMultiPartUploadHandler> logger,
        IS3Provider s3Provider)
    {
        _logger = logger;
        _s3Provider = s3Provider;
    }

    public async Task Handle(StartMultiPartUploadRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        //var startUploadResult = await _s3Provider.StartMultiPartUploadAsync();
        
    }
}
using AuthService.Contracts.Dtos.ReconfirmEmail;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.ReconfirmEmail;

public sealed class ReconfirmEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/resend-confirmation", async Task<EndpointResult>(
            [FromBody] ReconfirmEmailRequest request,
            [FromServices] ReconfirmEmailHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ReconfirmEmailCommand(request);
            return await handler.Handle(command, cancellationToken);
        });
    }
}
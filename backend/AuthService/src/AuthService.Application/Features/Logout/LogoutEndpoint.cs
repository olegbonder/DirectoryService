using AuthService.Contracts.Dtos.Logout;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Logout;

public sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", async Task<EndpointResult>(
            [FromBody] LogoutRequest request,
            [FromServices] LogoutHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LogoutCommand(request);
            return await handler.Handle(command, cancellationToken);
        }).RequireAuthorization();
    }
}
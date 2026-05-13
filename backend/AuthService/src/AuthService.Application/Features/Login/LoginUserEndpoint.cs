using AuthService.Contracts.Dtos.Login;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Login;

public sealed class LoginUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async Task<EndpointResult<LoginResponse>>(
            [FromBody] LoginRequest request,
            [FromServices] LoginUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request);
            return await handler.Handle(command, cancellationToken);
        });
    }
}
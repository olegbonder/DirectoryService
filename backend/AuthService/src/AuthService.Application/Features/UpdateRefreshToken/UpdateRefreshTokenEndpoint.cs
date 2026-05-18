using AuthService.Contracts.Dtos.Login;
using AuthService.Contracts.Dtos.UpdateRefreshToken;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.UpdateRefreshToken;

public sealed class UpdateRefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async Task<EndpointResult<LoginResponse>>(
            [FromBody] UpdateRefreshTokenRequest request,
            [FromServices] UpdateRefreshTokenHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateRefreshTokenCommand(request);
            return await handler.Handle(command, cancellationToken);
        }).AllowAnonymous();
    }
}
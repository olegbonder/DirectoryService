using AuthService.Contracts.Dtos.RegisterUser;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.RegisterUser;

public sealed class RegisterUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async Task<EndpointResult<Guid>>(
            [FromBody] RegisterUserRequest request,
            [FromServices] RegisterUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(request);
            return await handler.Handle(command, cancellationToken);
        }).AllowAnonymous();
    }
}
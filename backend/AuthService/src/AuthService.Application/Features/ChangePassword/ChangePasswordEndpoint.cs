using AuthService.Contracts.Dtos.ChangePassword;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.ChangePassword;

public sealed class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/auth/change-password",
            async Task<EndpointResult>(
                [FromBody] ChangePasswordRequest request,
                [FromServices] ChangePasswordHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new ChangePasswordCommand(request);
                return await handler.Handle(command, cancellationToken);
            }).RequireAuthorization();
    }
}
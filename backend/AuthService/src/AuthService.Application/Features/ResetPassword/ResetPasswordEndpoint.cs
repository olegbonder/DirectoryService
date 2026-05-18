using AuthService.Contracts.Dtos.ResetPassword;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.ResetPassword;

public sealed class ResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/auth/reset-password",
            async Task<EndpointResult>(
                [FromBody] ResetPasswordRequest request,
                [FromServices] ResetPasswordHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new ResetPasswordCommand(request);
                return await handler.Handle(command, cancellationToken);
            }).AllowAnonymous();
    }
}
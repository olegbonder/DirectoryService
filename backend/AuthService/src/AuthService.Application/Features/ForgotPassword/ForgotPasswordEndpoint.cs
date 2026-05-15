using AuthService.Contracts.Dtos.ForgotPassword;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.ForgotPassword;

public sealed class ForgotPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/forgot-password", async Task<EndpointResult>(
            [FromBody] ForgotPasswordRequest request,
            [FromServices] ForgotPasswordHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ForgotPasswordCommand(request);
            return await handler.Handle(command, cancellationToken);
        });
    }
}
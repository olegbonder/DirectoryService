using AuthService.Contracts.Dtos.VerifyEmail;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.VerifyEmail;

public sealed class VerifyEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/auth/confirm-email",
            async Task<EndpointResult<Guid>>(
                [AsParameters] VerifyEmailRequest request,
                [FromServices] VerifyEmailHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new VerifyEmailCommand(request);
                return await handler.Handle(command, cancellationToken);
            }).WithName("ConfirmEmail");
    }
}
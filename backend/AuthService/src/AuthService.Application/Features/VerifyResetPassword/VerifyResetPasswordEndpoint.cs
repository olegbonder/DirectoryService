using AuthService.Contracts.Dtos.VerifyEmail;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.VerifyResetPassword;

public sealed class VerifyResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/auth/reset-password",
            async Task<EndpointResult<VerifyEmailRequest>>(
                [AsParameters] VerifyEmailRequest request,
                [FromServices] VerifyResetPasswordHandler handler,
                CancellationToken cancellationToken) =>
            {
                return await handler.Handle(request, cancellationToken);
            }).WithName("VerifyResetPassword");
    }
}
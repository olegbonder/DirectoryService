using AuthService.Application.Permission;
using AuthService.Domain.Permissions;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Admin.DeactivateUser;

public sealed class DeactivateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(
            "/users/{userId:guid}/deactivate",
            async Task<EndpointResult>(
                [FromRoute] Guid userId,
                [FromServices] DeactivateUserHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeactivateUserCommand(userId);
                return await handler.Handle(command, cancellationToken);
            }).RequirePermissions(PlatformPermissions.USERS_MANAGE);
    }
}
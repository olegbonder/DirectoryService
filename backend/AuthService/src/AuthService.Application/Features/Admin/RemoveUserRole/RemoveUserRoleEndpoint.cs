using AuthService.Application.Permission;
using AuthService.Domain.Permissions;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Admin.RemoveUserRole;

public sealed class RemoveUserRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/users/{userId:guid}/roles/{role}",
            async Task<EndpointResult>(
                [FromRoute] Guid userId,
                [FromRoute] string role,
                [FromServices] RemoveUserRoleHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new RemoveUserRoleCommand(userId, role);
                return await handler.Handle(command, cancellationToken);
            }).RequirePermissions(PlatformPermissions.USERS_MANAGE);
    }
}
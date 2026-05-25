using AuthService.Contracts.Dtos.Admin.AddUserRole;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SharedAuth.Constants;
using SharedAuth.Endpoints;

namespace AuthService.Application.Features.Admin.AddUserRole;

public sealed class AddUserRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/users/{userId:guid}/roles",
            async Task<EndpointResult>(
                [FromRoute] Guid userId,
                [FromBody] AddUserRoleRequest request,
                [FromServices] AddUserRoleHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new AddUserRoleCommand(userId, request);
                return await handler.Handle(command, cancellationToken);
            }).RequirePermissions(PlatformPermissions.USERS_MANAGE);
    }
}
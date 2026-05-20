using AuthService.Application.Permission;
using AuthService.Contracts.Dtos.Admin.GetUsers;
using AuthService.Domain.Permissions;
using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SharedKernel.PaginationAndOrder;

namespace AuthService.Application.Features.Admin.GetUsers;

public sealed class GetUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/users",
            async Task<EndpointResult<PaginationResponse<UserDTO>>>(
                [AsParameters] GetUsersRequest request,
                [FromServices] GetUsersHandler handler,
                CancellationToken cancellationToken) =>
            {
                return await handler.Handle(request, cancellationToken);
            }).RequirePermissions(PlatformPermissions.USERS_MANAGE);
    }
}
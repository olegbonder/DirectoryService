using Framework.EndpointResult;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SharedAuth;

namespace AuthService.Application.Features.UserProfile
{
    public class UserProfileEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/auth/me",
                async Task<EndpointResult<UserInfo>>(
                    [FromServices] UserProfileHandler handler,
                    CancellationToken cancellationToken) =>
                    await handler.Handle(cancellationToken)).RequireAuthorization();
        }
    }
}
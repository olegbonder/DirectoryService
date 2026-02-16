using Microsoft.AspNetCore.Routing;

namespace Framework.Endpoints;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}
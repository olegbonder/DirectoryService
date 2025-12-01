using DirectoryService.Application.Features.Locations.Commands.CreateLocation;
using DirectoryService.Application.Features.Locations.Queries.GetLocations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presenters.EndpointResult;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController: ControllerBase
    {
        [HttpPost]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(409)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<Guid>> Create(
            [FromBody] CreateLocationRequest request,
            [FromServices] CreateLocationHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new CreateLocationCommand(request);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpGet]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<GetLocationsResponse>> GetLocations(
            [FromQuery] GetLocationsRequest request,
            [FromServices] GetLocationsHandler handler,
            CancellationToken cancellationToken)
        {
            return await handler.Handle(request, cancellationToken);
        }
    }
}

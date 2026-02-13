using DirectoryService.Application.Features.Locations.Commands.CreateLocation;
using DirectoryService.Application.Features.Locations.Commands.SoftDeleteLocation;
using DirectoryService.Application.Features.Locations.Commands.UpdateLocation;
using DirectoryService.Application.Features.Locations.Queries.GetLocations;
using DirectoryService.Application.Features.Locations.Queries.GettLocationDictionary;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Contracts.Locations.GetLocationDictionary;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Presenters.EndpointResult;
using Microsoft.AspNetCore.Mvc;
using Shared;

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
        public async Task<EndpointResult<PaginationResponse<LocationDTO>>> GetLocations(
            [FromQuery] GetLocationsRequest request,
            [FromServices] GetLocationsHandler handler,
            CancellationToken cancellationToken)
        {
            return await handler.Handle(request, cancellationToken);
        }

        [HttpGet("dictionary")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<PaginationResponse<DictionaryItemResponse>>> GetLocationDictionary(
            [FromQuery] GetLocationDictionaryRequest request,
            [FromServices] GettLocationDictionaryHandler handler,
            CancellationToken cancellationToken)
        {
            return await handler.Handle(request, cancellationToken);
        }

        [HttpPut("{locationId:guid}")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<Guid>> Update(
            [FromRoute] Guid locationId,
            [FromBody] UpdateLocationRequest request,
            [FromServices] UpdateLocationHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new UpdateLocationCommand(locationId, request);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpDelete("{locationId:guid}")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<Guid>> SoftDelete(
            [FromRoute] Guid locationId,
            [FromServices] SoftDeleteLocationHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new SoftDeleteLocationCommand(locationId);
            return await handler.Handle(command, cancellationToken);
        }
    }
}

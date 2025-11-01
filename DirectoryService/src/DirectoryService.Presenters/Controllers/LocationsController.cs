using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presenters.EndpointResult;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController: ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpPost]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<Guid>> Create([FromBody] CreateLocationDTO locationDTO, CancellationToken cancellationToken)
        {
            return await _locationService.Create(locationDTO, cancellationToken);
        }
    }
}

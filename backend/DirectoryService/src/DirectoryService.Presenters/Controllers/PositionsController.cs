using DirectoryService.Application.Features.Positions.Commands.CreatePosition;
using DirectoryService.Contracts.Positions.CreatePosition;
using DirectoryService.Presenters.EndpointResult;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionsController: ControllerBase
    {
        [HttpPost]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(409)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<Guid>> Create(
            [FromBody] CreatePositionRequest request,
            [FromServices] CreatePositionHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new CreatePositionCommand(request);
            return await handler.Handle(command, cancellationToken);
        }
    }
}

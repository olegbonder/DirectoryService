using DirectoryService.Application.Features.Positions.Commands.CreatePosition;
using DirectoryService.Application.Features.Positions.Commands.CreatePositionDepartments;
using DirectoryService.Application.Features.Positions.Commands.DeletePositionDepartment;
using DirectoryService.Application.Features.Positions.Commands.SoftDeletePosition;
using DirectoryService.Application.Features.Positions.Commands.UpdatePosition;
using DirectoryService.Application.Features.Positions.Queries.GetPositionDetail;
using DirectoryService.Application.Features.Positions.Queries.GetPositions;
using DirectoryService.Contracts.Locations.UpdatePosition;
using DirectoryService.Contracts.Positions.CreatePosition;
using DirectoryService.Contracts.Positions.CreatePositionDepartments;
using DirectoryService.Contracts.Positions.GetPosition;
using DirectoryService.Contracts.Positions.GetPositions;
using Framework.EndpointResult;
using Microsoft.AspNetCore.Mvc;
using Shared;
using SharedKernel;

namespace DirectoryService.Presenters.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionsController: ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<PaginationResponse<PositionDTO>>> GetPositions(
            [FromQuery] GetPositionsRequest request,
            [FromServices] GetPositionsHandler handler,
            CancellationToken cancellationToken)
        {
            return await handler.Handle(request, cancellationToken);
        }

        [HttpGet("{positionId:guid}")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<PositionDetailDTO?>> GetPosition(
            [FromRoute] Guid positionId,
            [FromServices] GetPositionDetailHandler handler,
            CancellationToken cancellationToken)
        {
            var query = new GetPositionRequest(positionId);
            return await handler.Handle(query, cancellationToken);
        }

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

        [HttpPatch("{positionId:guid}")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<Guid>> Update(
            [FromRoute] Guid positionId,
            [FromBody] UpdatePositionRequest request,
            [FromServices] UpdatePositionHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new UpdatePositionCommand(positionId, request);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpDelete("{positionId:guid}")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<Guid>> SoftDelete(
            [FromRoute] Guid positionId,
            [FromServices] SoftDeletePositionHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new SoftDeletePositionCommand(positionId);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpDelete("{positionId:guid}/departments/{departmentId:guid}")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<Guid>> DeletePositionDepartment(
            [FromRoute] Guid positionId,
            [FromRoute] Guid departmentId,
            [FromServices] DeletePositionDepartmentHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new DeletePositionDepartmentCommand(positionId, departmentId);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpPost("{positionId:guid}/departments")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<Guid>> CreatePositionDepartments(
            [FromRoute] Guid positionId,
            [FromBody] CreatePositionDepartmentsRequest request,
            [FromServices] CreatePositionDepartmentsHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new CreatePositionDepartmentsCommand(positionId, request);
            return await handler.Handle(command, cancellationToken);
        }
    }
}

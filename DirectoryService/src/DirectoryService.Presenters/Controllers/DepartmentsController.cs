using DirectoryService.Application.Features.Departments.Commands.CreateDepartment;
using DirectoryService.Application.Features.Departments.Commands.MoveDepartment;
using DirectoryService.Application.Features.Departments.Commands.UpdateDepartmentLocations;
using DirectoryService.Application.Features.Departments.Queries.GetTopDepartments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presenters.EndpointResult;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController: ControllerBase
    {
        [HttpPost]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<Guid>> Create(
            [FromBody] CreateDepartmentRequest request,
            [FromServices] CreateDepartmentHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new CreateDepartmentCommand(request);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpPut("{departmentId:guid}/locations")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<Guid>> UpdateDeaprtmentLocations(
            [FromRoute] Guid departmentId,
            [FromBody] UpdateDepartmentLocationsRequest request,
            [FromServices] UpdateDepartmentLocationsHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new UpdateLocationsCommand(departmentId, request);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpPut("{departmentId:guid}/parent")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<Guid>> MoveDeaprtment(
            [FromRoute] Guid departmentId,
            [FromBody] MoveDepartmentRequest request,
            [FromServices] MoveDepartmentHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new MoveDepartmentCommand(departmentId, request);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpGet("top-positions")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<GetTopDepartmentsResponse>> GetTopDepartments(
            [FromQuery] GetTopDepartmentsRequest request,
            [FromServices] GetTopDepartmentsHandler handler,
            CancellationToken cancellationToken)
        {
            return await handler.Handle(request, cancellationToken);
        }
    }
}

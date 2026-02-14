using DirectoryService.Application.Features.Departments.Commands.CreateDepartment;
using DirectoryService.Application.Features.Departments.Commands.MoveDepartment;
using DirectoryService.Application.Features.Departments.Commands.SoftDeleteDepartment;
using DirectoryService.Application.Features.Departments.Commands.UpdateDepartment;
using DirectoryService.Application.Features.Departments.Commands.UpdateDepartmentLocations;
using DirectoryService.Application.Features.Departments.Queries.GetDepartmentDetail;
using DirectoryService.Application.Features.Departments.Queries.GetDepartmentDictionary;
using DirectoryService.Application.Features.Departments.Queries.GetDepartments;
using DirectoryService.Application.Features.Departments.Queries.GetTopDepartments;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.GetChildDepartments;
using DirectoryService.Contracts.Departments.GetDepartment;
using DirectoryService.Contracts.Departments.GetDepartmentDictionary;
using DirectoryService.Contracts.Departments.GetDepartments;
using DirectoryService.Contracts.Departments.GetRootDepartments;
using DirectoryService.Contracts.Departments.GetTopDepartments;
using DirectoryService.Contracts.Departments.MoveDepartment;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using DirectoryService.Contracts.Departments.UpdateDepartmentLocations;
using Framework.EndpointResult;
using Microsoft.AspNetCore.Mvc;
using Shared;
using SharedKernel;

namespace DirectoryService.Presenters.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController: ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<PaginationResponse<DepartmentDTO>>> GetDepartments(
            [FromQuery] GetDepartmentsRequest request,
            [FromServices] GetDepartmentsHandler handler,
            CancellationToken cancellationToken)
        {
            return await handler.Handle(request, cancellationToken);
        }

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
        public async Task<EndpointResult<Guid>> UpdateDepartmentLocations(
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
        public async Task<EndpointResult<Guid>> MoveDepartment(
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

        [HttpGet("roots")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<PaginationResponse<RootDepartmentDTO>>> GetRootDepartments(
            [FromQuery] GetRootDepartmentsRequest request,
            [FromServices] GetRootDepartmentsHandler handler,
            CancellationToken cancellationToken)
        {
            return await handler.Handle(request, cancellationToken);
        }

        [HttpGet("dictionary")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<PaginationResponse<DictionaryItemResponse>>> GetDepartmentDictionary(
            [FromQuery] GetDepartmentDictionaryRequest request,
            [FromServices] GetDepartmentDictionaryHandler handler,
            CancellationToken cancellationToken)
        {
            return await handler.Handle(request,cancellationToken);
        }

        [HttpGet("{parentId:guid}/children")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<PaginationResponse<ChildDepartmentDTO>>> GetChildDepartments(
            [FromRoute] Guid parentId,
            [FromQuery] PaginationRequest request,
            [FromServices] GetChildDepartmentsHandler handler,
            CancellationToken cancellationToken)
        {
            var query = new GetChildDepartmentsQuery(parentId, request);
            return await handler.Handle(query, cancellationToken);
        }

        [HttpGet("{departmentId:guid}")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(400)]
        [ProducesResponseType<Envelope>(404)]
        [ProducesResponseType<Envelope>(500)]
        public async Task<EndpointResult<DepartmentDetailDTO?>> GetPosition(
            [FromRoute] Guid departmentId,
            [FromServices] GetDepartmentDetailHandler handler,
            CancellationToken cancellationToken)
        {
            var query = new GetDepartmentRequest(departmentId);
            return await handler.Handle(query, cancellationToken);
        }

        [HttpPatch("{departmentId:guid}")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<Guid>> Update(
            [FromRoute] Guid departmentId,
            [FromBody] UpdateDepartmentRequest request,
            [FromServices] UpdateDepartmentHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new UpdateDepartmentCommand(departmentId, request);
            return await handler.Handle(command, cancellationToken);
        }

        [HttpDelete("{departmentId:guid}")]
        [ProducesResponseType<Envelope<Guid>>(200)]
        [ProducesResponseType<Envelope>(404)]
        public async Task<EndpointResult<Guid>> SoftDelete(
            [FromRoute] Guid departmentId,
            [FromServices] SoftDeleteDepartmentHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new SoftDeleteDepartmentCommand(departmentId);
            return await handler.Handle(command, cancellationToken);
        }
    }
}

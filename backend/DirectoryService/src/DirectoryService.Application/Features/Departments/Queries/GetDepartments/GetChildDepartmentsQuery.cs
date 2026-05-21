using SharedKernel.PaginationAndOrder;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartments;

public record GetChildDepartmentsQuery(Guid ParentId, PaginationRequest Request);
using DirectoryService.Contracts.Departments.GetChildDepartments;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartments;

public record GetChildDepartmentsQuery
{
    public GetChildDepartmentsQuery(Guid parentId, GetChildDepartmentsRequest? request)
    {
        ParentId = parentId;
        Request = request ?? new GetChildDepartmentsRequest();
    }

    public Guid ParentId { get; init; }
    public GetChildDepartmentsRequest Request { get; init; }
}
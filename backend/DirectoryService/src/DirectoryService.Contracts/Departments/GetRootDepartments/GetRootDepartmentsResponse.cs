using DirectoryService.Contracts.Departments.GetChildDepartments;

namespace DirectoryService.Contracts.Departments.GetRootDepartments
{
    public record GetRootDepartmentsResponse(List<RootDepartmentDTO> Departments, int TotalCount);
}

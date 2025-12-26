namespace DirectoryService.Contracts.Departments.GetChildDepartments
{
    public record GetChildDepartmentsResponse(List<ChildDepartmentDTO> Departments, int TotalCount);
}

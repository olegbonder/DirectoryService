namespace DirectoryService.Contracts.Departments.GetTopDepartments
{
    public record GetTopDepartmentsResponse(List<TopDepartmentDTO> Departments, int TotalCount);
}

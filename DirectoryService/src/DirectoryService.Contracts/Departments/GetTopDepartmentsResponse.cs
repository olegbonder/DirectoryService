namespace DirectoryService.Contracts.Departments
{
    public record GetTopDepartmentsResponse(List<TopDepartmentDTO> Departments, int TotalCount);
}

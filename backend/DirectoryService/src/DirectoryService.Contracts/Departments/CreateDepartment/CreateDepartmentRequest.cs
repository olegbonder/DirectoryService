namespace DirectoryService.Contracts.Departments.CreateDepartment
{
    public record CreateDepartmentRequest(string Name, string Identifier, Guid? ParentId, List<Guid> LocationIds);
}

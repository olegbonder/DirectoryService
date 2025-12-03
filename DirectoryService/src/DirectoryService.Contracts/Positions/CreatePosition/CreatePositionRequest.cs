namespace DirectoryService.Contracts.Positions.CreatePosition
{
    public record CreatePositionRequest(string Name, string? Description, List<Guid> DepartmentIds);
}

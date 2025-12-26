namespace DirectoryService.Contracts.Departments.GetTopDepartments;

public class TopDepartmentDTO
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string Path { get; init; } = null!;

    public int Depth { get; init; }

    public DateTime CreatedAt { get; init; }

    public int PositionsCount { get; init; }
}
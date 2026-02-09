using DirectoryService.Contracts.Departments.GetChildDepartments;

namespace DirectoryService.Contracts.Departments.GetRootDepartments;

public class RootDepartmentDTO
{
    public RootDepartmentDTO()
    {
        Children = new List<ChildDepartmentDTO>();
    }

    public Guid Id { get; init; }

    public Guid? ParentId { get; init; }

    public string Name { get; init; } = null!;

    public string Identifier { get; init; } = null!;

    public string Path { get; init; } = null!;

    public int Depth { get; init; }

    public DateTime CreatedAt { get; init; }

    public bool IsActive { get; init; }

    public List<ChildDepartmentDTO> Children { get; init; }

    public bool HasMoreChildren { get; init; }
}
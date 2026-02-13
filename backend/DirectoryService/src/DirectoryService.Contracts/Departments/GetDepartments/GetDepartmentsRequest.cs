using System.Text.Json.Serialization;

namespace DirectoryService.Contracts.Departments.GetDepartments
{
    public record GetDepartmentsRequest : PaginationRequest
    {
        public string? Name { get; init; }

        public string? Identifier { get; init; }

        public Guid? ParentId { get; init; }

        public List<Guid>? LocationIds { get; init; }

        public bool? IsActive { get; init; }

        public DepartmentOrderField? OrderBy { get; init; }

        public OrderDirection? OrderDirection { get; init; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DepartmentOrderField
    {
        Name,
        Path,
        CreatedAt
    }
}

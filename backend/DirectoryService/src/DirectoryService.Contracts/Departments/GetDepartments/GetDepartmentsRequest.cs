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

        public OrderColumn? OrderColumn { get; init; }
    }

    public record OrderColumn
    {
        public OrderField Field { get; init; }
        public OrderDirection Direction { get; init; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderField
    {
        Name,
        Path,
        CreatedAt
    }
}

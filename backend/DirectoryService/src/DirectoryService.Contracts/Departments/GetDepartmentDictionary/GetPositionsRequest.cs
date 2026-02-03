namespace DirectoryService.Contracts.Departments.GetDepartmentDictionary
{
    public record GetDepartmentDictionaryRequest : PaginationRequest
    {
        public string? Search { get; init; }
        public List<Guid>? DepartmentIds { get; init; }
    }
}

namespace DirectoryService.Contracts.Departments.GetRootDepartments
{
    public record GetRootDepartmentsRequest : PaginationRequest
    {
        public int? Prefetch { get; set; } = 3;
    }
}

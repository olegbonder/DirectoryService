namespace DirectoryService.Contracts.Departments.GetRootDepartments
{
    public record GetRootDepartmentsRequest
    {
        public GetRootDepartmentsRequest()
        {
            Pagination = new PaginationRequest();
        }

        public PaginationRequest Pagination { get; init; }
        public int? Prefetch { get; set; } = 3;
    }
}

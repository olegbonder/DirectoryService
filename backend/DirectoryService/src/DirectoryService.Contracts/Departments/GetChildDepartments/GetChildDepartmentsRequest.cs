namespace DirectoryService.Contracts.Departments.GetChildDepartments
{
    public record GetChildDepartmentsRequest
    {
        public GetChildDepartmentsRequest()
        {
            Pagination = new PaginationRequest();
        }

        public PaginationRequest Pagination { get; init; }
    }
}

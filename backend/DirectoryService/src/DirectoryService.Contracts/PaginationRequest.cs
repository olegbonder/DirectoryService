namespace DirectoryService.Contracts
{
    public record PaginationRequest
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}

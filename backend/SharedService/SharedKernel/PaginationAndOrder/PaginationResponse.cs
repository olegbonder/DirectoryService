namespace SharedKernel.PaginationAndOrder
{
    public record PaginationResponse<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages);
}
using SharedKernel.PaginationAndOrder;

namespace AuthService.Contracts.Dtos.Admin.GetUsers
{
    public record GetUsersRequest : PaginationRequest
    {
        public string? Name { get; init; }
        public string? Email { get; init; }
    }
}
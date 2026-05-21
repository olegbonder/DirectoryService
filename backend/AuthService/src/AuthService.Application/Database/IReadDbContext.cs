using AuthService.Domain;

namespace AuthService.Application.Database
{
    public interface IReadDbContext
    {
        IQueryable<ApplicationUser> UsersRead { get; }
    }
}

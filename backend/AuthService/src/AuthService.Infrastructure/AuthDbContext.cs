using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Domain.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure;

public class AuthDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IReadDbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }

    protected ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder => builder.AddConsole());
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public IQueryable<ApplicationUser> UsersRead => Set<ApplicationUser>().AsQueryable().AsNoTracking();
}
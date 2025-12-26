using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    public static class MigrateExtension
    {
        public static async Task<IServiceProvider> RunMigrating(this IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            return services;
        }
    }
}

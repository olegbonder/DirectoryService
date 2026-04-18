using Microsoft.Extensions.DependencyInjection;

namespace FileService.Infrastructure.Postgres.Initializers
{
    public static class QuartzExtension
    {
        public static async Task RunQuartzDbInitializer(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<QuartzDbInitializer>();
            await dbContext.InitializeAsync();
        }
    }
}

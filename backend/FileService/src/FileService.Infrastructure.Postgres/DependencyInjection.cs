using Core.Database;
using DirectoryService.Infrastructure.Postgres;
using FileService.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Infrastructure.Postgres
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(Constants.DATABASE_CONNECTIONSTRING);
            services.AddScoped(s => new FileServiceDbContext(connectionString!));

            services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();

            return services;
        }
    }
}

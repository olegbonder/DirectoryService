using DirectoryService.Application.Features.Departments;
using DirectoryService.Application.Features.Locations;
using DirectoryService.Infrastructure.Postgres.Departments;
using DirectoryService.Infrastructure.Postgres.Locations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(s => new ApplicationDbContext(configuration));
            services.AddScoped<ILocationsRepository, LocationsRepository>();
            services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();

            return services;
        }
    }
}

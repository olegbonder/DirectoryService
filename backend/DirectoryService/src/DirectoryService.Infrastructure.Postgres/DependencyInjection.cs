using Core.Database;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Features.Departments;
using DirectoryService.Application.Features.Locations;
using DirectoryService.Application.Features.Positions;
using DirectoryService.Infrastructure.Postgres.BackgroundServices;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Departments;
using DirectoryService.Infrastructure.Postgres.Locations;
using DirectoryService.Infrastructure.Postgres.Positions;
using DirectoryService.Infrastructure.Postgres.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(Constants.DATABASE_CONNECTIONSTRING);
            services.AddScoped(s => new ApplicationDbContext(connectionString!));
            services.AddScoped<IReadDbContext, ApplicationDbContext>(s => new ApplicationDbContext(connectionString!));
            services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>(s => new NpgsqlConnectionFactory(connectionString!));
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            services.AddScoped<ITransactionManager, TransactionManager>();
            services.AddScoped<TransactionManager>();
            services.AddScoped<ILocationsRepository, LocationsRepository>();
            services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
            services.AddScoped<IPositionsRepository, PositionsRepository>();

            services.AddScoped<ISeeder, Seeder>();

            services.AddScoped<DeleteExpiredDepartmentService>();
            services.AddHostedService<DeleteExpiredDepartmentBackgroundService>();

            return services;
        }
    }
}

using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Features.Departments;
using DirectoryService.Application.Features.Locations;
using DirectoryService.Application.Features.Positions;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Departments;
using DirectoryService.Infrastructure.Postgres.Locations;
using DirectoryService.Infrastructure.Postgres.Positions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres
{
    public static class DependencyInjection
    {
        private const string DATABASE_CONNECTIONSTRING = "DirectoryServiceDb";

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(DATABASE_CONNECTIONSTRING);
            services.AddScoped(s => new ApplicationDbContext(connectionString!));
            services.AddScoped<IReadDbContext, ApplicationDbContext>(s => new ApplicationDbContext(connectionString!));
            services.AddSingleton<IDBConnectionFactory, NpgsqlConnectionFactory>(s => new NpgsqlConnectionFactory(connectionString!));
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            services.AddScoped<ITransactionManager, TransactionManager>();
            services.AddScoped<ILocationsRepository, LocationsRepository>();
            services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
            services.AddScoped<IPositionsRepository, PositionsRepository>();

            return services;
        }
    }
}

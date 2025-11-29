using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests.Infrasructure
{
    public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres")
            .WithDatabase("directory_service_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private Respawner _respawner = null!;
        private DbConnection _dbConnection = null!;

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());

            await InitializeRespawner();
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();

            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            await _respawner.ResetAsync(_dbConnection);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ApplicationDbContext>();
                services.RemoveAll<IDBConnectionFactory>();

                var connectionString = _dbContainer.GetConnectionString();
                services.AddScoped(_ =>
                    new ApplicationDbContext(connectionString));
                services.AddSingleton<IDBConnectionFactory, NpgsqlConnectionFactory>(s =>
                    new NpgsqlConnectionFactory(connectionString!));
            });
        }

        private async Task InitializeRespawner()
        {
            await _dbConnection.OpenAsync();
            _respawner = await Respawner.CreateAsync(
                _dbConnection,
                new RespawnerOptions
                {
                    DbAdapter = DbAdapter.Postgres,
                    SchemasToInclude = [ "public" ]
                });
        }
    }
}

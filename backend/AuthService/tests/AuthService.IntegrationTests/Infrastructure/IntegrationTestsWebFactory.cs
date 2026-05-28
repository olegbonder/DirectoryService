using System.Data.Common;
using AuthService.Application.Database;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;
using SharedAuth.Constants;
using Testcontainers.PostgreSql;

namespace AuthService.IntegrationTests.Infrastructure;

public class IntegrationTestsWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("auth_service_db_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Tests");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.IntegrationTests.json"), optional: true);
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _dbContainer.GetConnectionString()
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AuthDbContext>();
            services.RemoveAll<IReadDbContext>();

            services.AddDbContextPool<AuthDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            services.AddDbContextPool<IReadDbContext, AuthDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Remove hosted services so background initialization (roles, buckets, etc.) doesn't run in tests
            services.RemoveAll<IHostedService>();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        await InitializeRespawner();

        //await CreateRolesAsync(scope);
    }

    private async Task InitializeRespawner()
    {
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
    }



    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
}
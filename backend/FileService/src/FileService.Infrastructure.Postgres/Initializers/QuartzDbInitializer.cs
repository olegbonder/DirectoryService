using System.Reflection;
using FileService.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FileService.Infrastructure.Postgres.Initializers;

public class QuartzDbInitializer
{
    private readonly ILogger<QuartzDbInitializer> _logger;
    private readonly string? _connectionString;

    public QuartzDbInitializer(IConfiguration configuration, ILogger<QuartzDbInitializer> logger)
    {
        _connectionString = configuration.GetConnectionString(ConnectionStringNames.DATABASE);
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            string sqlScript = await LoadSqlScriptAsync();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(stoppingToken);

#pragma warning disable CA2100
            await using var command = new NpgsqlCommand(sqlScript, connection);
#pragma warning restore CA2100

            await command.ExecuteNonQueryAsync(stoppingToken);

            _logger.LogInformation("Quartz database initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Quartz tables");
            throw;
        }
    }

    private static async Task<string> LoadSqlScriptAsync()
    {
        Assembly assembly = typeof(QuartzDbInitializer).Assembly;
        string resourceName = "FileService.Infrastructure.Postgres.Scripts.quartz_postgres.sql";

        await using Stream? stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
        }

        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }
}
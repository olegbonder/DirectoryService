using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Infrastructure.Postgres.Migrations;
using DirectoryService.Infrastructure.Postgres.Seeding;
using DirectoryService.Web.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    string environmentName = builder.Environment.EnvironmentName;
    builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);

    builder.Services.AddProgramDependencies(builder.Configuration);

    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    app.ConfigureApp();

    if (app.Environment.IsDevelopment())
    {
        if (args.Contains("--seeding"))
        {
            await app.Services.RunSeeding();
        }
    }
    else
    {
        await app.Services.RunMigrating();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

namespace DirectoryService.Web
{
    public partial class Program;
}
using FileService.Core.Messaging;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.Postgres.Migrations;
using FileService.Infrastructure.S3;
using FileService.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
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

    builder.AddWolverine();
    var app = builder.Build();

    app.ConfigureApp();

    if (app.Environment.IsDevelopment())
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

public partial class Program;
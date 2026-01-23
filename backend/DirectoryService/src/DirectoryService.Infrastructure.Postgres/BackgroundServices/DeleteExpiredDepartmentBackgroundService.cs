using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.BackgroundServices;

public class DeleteExpiredDepartmentBackgroundService : BackgroundService
{
    private readonly ILogger<DeleteExpiredDepartmentBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DeleteExpiredDepartmentBackgroundService(
        ILogger<DeleteExpiredDepartmentBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting delete expired department background service");

        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var deleteDepartmentService = scope.ServiceProvider.GetRequiredService<DeleteExpiredDepartmentService>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            int deleteExpiredDepartmentHours = configuration.GetValue("DeleteExpiredDepartmentHours", 1);

            await deleteDepartmentService.Process(stoppingToken);

            await Task.Delay(TimeSpan.FromHours(deleteExpiredDepartmentHours), stoppingToken);
        }

        await Task.CompletedTask;
    }
}
using System.Text.Json;
using AuthService.Application;
using AuthService.Application.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace AuthService.Infrastructure.Jobs;

public class CleanUpRefreshTokenJob : IJob
{
    private readonly ILogger<CleanUpRefreshTokenJob> _logger;
    private readonly IRefreshTokenRepository _repository;
    private readonly ITransactionManager _transactionManager;
    private readonly CleanUpRefreshTokenJobOptions _options;

    public CleanUpRefreshTokenJob(
        ILogger<CleanUpRefreshTokenJob> logger,
        IRefreshTokenRepository repository,
        ITransactionManager transactionManager,
        IOptions<CleanUpRefreshTokenJobOptions> options)
    {
        _logger = logger;
        _repository = repository;
        _transactionManager = transactionManager;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting delete refresh token job");

        var currentDateTime = DateTime.UtcNow;
        var minRefreshTokenRevokeDate = currentDateTime.AddDays(-_options.RevokeTokenOlderThanDays);
        var expiredTokens = await _repository
            .GetCollectionBy(rt => rt.ExpiresAt < currentDateTime, context.CancellationToken);
        if (expiredTokens.Any())
        {
            _repository.DeleteTokensAsync(expiredTokens);
        }

        var revokedTokens = await _repository
            .GetCollectionBy(rt => rt.RevokedAt < minRefreshTokenRevokeDate, context.CancellationToken);
        if (revokedTokens.Any())
        {
            _repository.DeleteTokensAsync(revokedTokens);
        }

        var result = await _transactionManager.SaveChangesAsync(context.CancellationToken);
        if (result.IsFailure)
        {
            var errors = result.Errors.Select(e => new { e.Code, e.Message });
            string errorMessage = JsonSerializer.Serialize(errors);
            _logger.LogError("Delete refresh token job failed: {Error}", errorMessage);

            throw new JobExecutionException(refireImmediately: false);
        }

        _logger.LogInformation("Delete refresh tokens job completed successfully");
    }
}

using System.Data;
using DirectoryService.Application.Abstractions.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Infrastructure.Postgres.Database
{
    public class TransactionManager: ITransactionManager
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TransactionManager> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public TransactionManager(
            ApplicationDbContext dbContext,
            ILogger<TransactionManager> logger,
            ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public async Task<Result<ITransactionScope>> BeginTransaction(IsolationLevel? level = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync(level ?? IsolationLevel.ReadCommitted, cancellationToken);

                var transactionScopeLogger = _loggerFactory.CreateLogger<TransactionScope>();
                var transactionScope = new TransactionScope(transaction.GetDbTransaction(), transactionScopeLogger);

                return transactionScope;
            }
            catch (Exception ex)
            {
                var msg = "Failed to begin transaction";
                _logger.LogError(ex, msg);
                return Error.Failure("database", msg);
            }
        }

        public async Task SaveChanges(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

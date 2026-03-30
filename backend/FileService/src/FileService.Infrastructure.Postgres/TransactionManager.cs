using System.Data;
using FileService.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Infrastructure.Postgres
{
    public class TransactionManager : ITransactionManager
    {
        private readonly FileServiceDbContext _dbContext;
        private readonly ILogger<TransactionManager> _logger;

        public TransactionManager(FileServiceDbContext dbContext, ILogger<TransactionManager> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            return transaction.GetDbTransaction();
        }

        public async Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while saving changes.");
                return Error.Conflict("concurrency.error", "Concurrency conflict while saving changes.");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Operation canceled while saving changes.");
                return GeneralErrors.Failure("save.changes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database.");
                return GeneralErrors.Failure("save.changes");
            }
        }
    }
}
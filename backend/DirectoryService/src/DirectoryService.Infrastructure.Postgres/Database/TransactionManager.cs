using System.Data.Common;
using DirectoryService.Application.Abstractions.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;
using Wolverine.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Database
{
    public class TransactionManager : ITransactionManager
    {
        private readonly IDbContextOutbox<ApplicationDbContext> _outbox;
        private readonly ILogger<TransactionManager> _logger;
        private IDbContextTransaction? _currentTransaction;

        public TransactionManager(
            IDbContextOutbox<ApplicationDbContext> dbContextOutbox,
            ILogger<TransactionManager> logger)
        {
            _outbox = dbContextOutbox;
            _logger = logger;
        }

        public async Task<Result> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                _currentTransaction = await _outbox.DbContext.Database.BeginTransactionAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error beginning transaction.");
                return GeneralErrors.Failure("transaction.begin");
            }
        }

        public async Task<Result> CommitTransactionAsync(CancellationToken cancellationToken)
        {
            if (_currentTransaction == null)
                return GeneralErrors.Failure("transaction.begin");

            try
            {
                await _outbox.DbContext.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
                await _outbox.FlushOutgoingMessagesAsync();
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while saving changes.");
                await RollbackAsync(cancellationToken);
                return GeneralErrors.ConcurrentOperation("concurrency.conflict");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Operation canceled while saving changes.");
                await RollbackAsync(cancellationToken);
                return GeneralErrors.OperationCancelled("save.changes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction.");
                await RollbackAsync(cancellationToken);
                return GeneralErrors.Failure("transaction.rollback");
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_currentTransaction != null)
                    await _currentTransaction.RollbackAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rollback transaction.");
            }
        }

        public DbConnection GetDbConnection() => _outbox.DbContext.Database.GetDbConnection();

        public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _outbox.DbContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    await _outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
                }

                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while saving changes.");
                return GeneralErrors.ConcurrentOperation("concurrency.error");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Operation canceled while saving changes.");
                return GeneralErrors.OperationCancelled("save.changes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database.");
                return GeneralErrors.Failure("save.changes");
            }
        }
    }
}

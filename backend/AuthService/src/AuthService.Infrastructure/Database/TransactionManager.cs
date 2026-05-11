using System.Data.Common;
using AuthService.Application.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace AuthService.Infrastructure.Database;

public class TransactionManager : ITransactionManager
    {
        private readonly AuthDbContext _dbContext;
        private readonly ILogger<TransactionManager> _logger;
        private IDbContextTransaction? _currentTransaction;

        public TransactionManager(
            AuthDbContext dbContext,
            ILogger<TransactionManager> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
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
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while saving changes.");
                await RollbackAsync(cancellationToken);
                return GeneralErrors.Failure("concurrency conflict");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Operation canceled while saving changes.");
                await RollbackAsync(cancellationToken);
                return GeneralErrors.Failure("save.changes");
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

        public DbConnection GetDbConnection() => _dbContext.Database.GetDbConnection();

        public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                return Result.Success();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while saving changes.");
                return GeneralErrors.Failure("concurrency.conflict");
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
    }
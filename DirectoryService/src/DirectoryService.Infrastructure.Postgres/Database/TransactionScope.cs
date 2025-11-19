using System.Data;
using DirectoryService.Application.Abstractions.Database;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Infrastructure.Postgres.Database
{

    public class TransactionScope : ITransactionScope
    {
        private readonly IDbTransaction _transaction;
        private readonly ILogger<TransactionScope> _logger;

        public TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
        {
            _transaction = transaction;
            _logger = logger;
        }

        public Result Commit()
        {
            try
            {
                _transaction.Commit();
                return Result.Success();
            }
            catch (Exception ex)
            {
                var msg = "Failed to commit transaction";
                _logger.LogError(ex, msg);
                return Error.Failure("transaction.commit.failed", msg);
            }
        }

        public Result RollBack()
        {
            try
            {
                _transaction.Rollback();
                return Result.Success();
            }
            catch (Exception ex)
            {
                var msg = "Failed to rollback transaction";
                _logger.LogError(ex, msg);
                return Error.Failure("transaction.rollback.failed", msg);
            }
        }

        public void Dispose() => _transaction.Dispose();
    }
}
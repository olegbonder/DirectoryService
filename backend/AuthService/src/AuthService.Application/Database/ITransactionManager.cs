using System.Data.Common;
using SharedKernel.Result;

namespace AuthService.Application.Database;

public interface ITransactionManager
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken);

    Task<Result> BeginTransactionAsync(CancellationToken cancellationToken);

    Task<Result> CommitTransactionAsync(CancellationToken cancellationToken);

    DbConnection GetDbConnection();
}
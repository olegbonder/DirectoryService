using System.Data.Common;
using SharedKernel.Result;

namespace DirectoryService.Application.Abstractions.Database
{
    public interface ITransactionManager
    {
        Task<Result> SaveChangesAsync(CancellationToken cancellationToken);

        Task<Result> BeginTransactionAsync(CancellationToken cancellationToken);

        Task<Result> CommitTransactionAsync(CancellationToken cancellationToken);

        Task RollbackAsync(CancellationToken cancellationToken);

        DbConnection GetDbConnection();
    }
}

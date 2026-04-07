using System.Data.Common;
using SharedKernel.Result;

namespace FileService.Core.Database
{
    public interface ITransactionManager
    {
        Task<Result> SaveChangesAsync(CancellationToken cancellationToken);

        Task<Result> BeginTransactionAsync(CancellationToken cancellationToken);

        Task<Result> CommitTransactionAsync(CancellationToken cancellationToken);

        DbConnection GetDbConnection();
    }
}
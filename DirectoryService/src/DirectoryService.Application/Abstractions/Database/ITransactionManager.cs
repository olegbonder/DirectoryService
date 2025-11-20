using System.Data;
using Shared.Result;

namespace DirectoryService.Application.Abstractions.Database
{
    public interface ITransactionManager
    {
        Task<Result<ITransactionScope>> BeginTransaction(IsolationLevel? level = null, CancellationToken cancellationToken = default);

        Task SaveChanges(CancellationToken cancellationToken);
    }
}

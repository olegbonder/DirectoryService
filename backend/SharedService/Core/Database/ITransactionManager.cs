using System.Data;
using SharedKernel.Result;

namespace Core.Database
{
    public interface ITransactionManager
    {
        Task<Result<ITransactionScope>> BeginTransaction(IsolationLevel? level = null, CancellationToken cancellationToken = default);

        Task SaveChanges(CancellationToken cancellationToken);
    }
}

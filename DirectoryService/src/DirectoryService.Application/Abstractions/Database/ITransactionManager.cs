using Shared.Result;

namespace DirectoryService.Application.Abstractions.Database
{
    public interface ITransactionManager
    {
        Task<Result<ITransactionScope>> BeginTransaction(CancellationToken cancellationToken);

        Task SaveChanges(CancellationToken cancellationToken);
    }
}

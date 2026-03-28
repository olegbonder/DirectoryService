using System.Data;
using SharedKernel.Result;

namespace FileService.Core
{
    public interface ITransactionManager
    {
        public Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken);

        public Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}
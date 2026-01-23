using System.Data;

namespace DirectoryService.Application.Abstractions.Database
{
    public interface IDBConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
    }
}

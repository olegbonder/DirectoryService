using System.Data;
using DirectoryService.Application.Abstractions.Database;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres.Database
{
    public class NpgsqlConnectionFactory : IDBConnectionFactory, IDisposable, IAsyncDisposable
    {
        private readonly NpgsqlDataSource _dataSource;

        public NpgsqlConnectionFactory(string connnectionString)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connnectionString);
            dataSourceBuilder
                .UseLoggerFactory(CreateLoggerFactory());

            _dataSource = dataSourceBuilder.Build();
        }

        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default) =>
            await _dataSource.OpenConnectionAsync(cancellationToken);

        public void Dispose() => _dataSource.Dispose();

        public ValueTask DisposeAsync() => _dataSource.DisposeAsync();

        private ILoggerFactory CreateLoggerFactory()
        {
            return LoggerFactory.Create(builder => builder.AddConsole());
        }
    }
}
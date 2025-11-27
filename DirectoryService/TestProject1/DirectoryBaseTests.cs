using DirectoryService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1
{
    public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
    {
        private readonly Func<Task> _resetDatabase;

        protected readonly IServiceProvider Services;

        protected DirectoryBaseTests(DirectoryTestWebFactory factory)
        {
            Services = factory.Services;
            _resetDatabase = factory.ResetDatabaseAsync;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _resetDatabase();
        }

        protected async Task<T> ExecuteInDb<T>(Func<ApplicationDbContext, Task<T>> action)
        {
            await using var scope = Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await action(dbContext);
        }

        protected async Task ExecuteInDb(Func<ApplicationDbContext, Task> action)
        {
            await using var scope = Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await action(dbContext);
        }
    }
}

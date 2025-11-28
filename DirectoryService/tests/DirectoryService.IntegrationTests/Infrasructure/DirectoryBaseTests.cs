using DirectoryService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrasructure
{
    public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
    {
        private readonly Func<Task> _resetDatabase;

        protected IServiceProvider Services { get; set; }

        protected TestData TestData { get; set; }

        public DirectoryBaseTests(DirectoryTestWebFactory factory)
        {
            Services = factory.Services;
            _resetDatabase = factory.ResetDatabaseAsync;
            var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            TestData = new TestData(dbContext);
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

        protected async Task<T> ExecuteHandler<T, TCommandHandler>(Func<TCommandHandler, Task<T>> action)
            where TCommandHandler : class
        {
            await using var scope = Services.CreateAsyncScope();

            var sut = scope.ServiceProvider.GetRequiredService<TCommandHandler>();

            return await action(sut);
        }
    }
}

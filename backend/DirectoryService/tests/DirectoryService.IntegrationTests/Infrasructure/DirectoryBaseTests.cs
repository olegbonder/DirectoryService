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
            TestData = new TestData(Services);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _resetDatabase();
        }
    }
}

using Amazon.S3;
using Amazon.S3.Model;
using FileService.Domain;
using FileService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestsBase : IAsyncLifetime, IClassFixture<IntegrationTestsWebFactory>
{
    private readonly IntegrationTestsWebFactory _factory;

    protected FileServiceTestsBase(IntegrationTestsWebFactory factory)
    {
        _factory = factory;
        AppHttpClient = _factory.CreateClient();
        HttpClient = new HttpClient();
        Services = _factory.Services;
        TestData = new TestData(AppHttpClient, HttpClient);
    }

    protected HttpClient HttpClient { get; init; }

    protected IServiceProvider Services { get; init; }

    protected HttpClient AppHttpClient { get; init; }

    protected TestData TestData { get; init; }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    protected async Task ExecuteInDb(Func<FileServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        FileServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        await action(dbContext);
    }

    protected async Task ExecuteInS3(Func<IAmazonS3, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        IAmazonS3 s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();

        await action(s3Client);
    }

    protected async Task<GetObjectResponse> GetObjectInS3(StorageKey key, CancellationToken cancellationToken = default)
    {
        await using var scope = Services.CreateAsyncScope();

        var amazonS3Client = Services.GetRequiredService<IAmazonS3>();

        var objectResponse = await amazonS3Client.GetObjectAsync(
            key.Bucket,
            key.Value,
            cancellationToken);

        return objectResponse;
    }
}
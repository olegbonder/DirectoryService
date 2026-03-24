using Amazon.S3;
using Amazon.S3.Model;
using FileService.Domain;
using FileService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestsBase : IClassFixture<IntegrationTestsWebFactory>
{
    protected FileServiceTestsBase(IntegrationTestsWebFactory factory)
    {
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
        Services = factory.Services;
        TestData = new TestData(Services, AppHttpClient);
    }

    protected HttpClient HttpClient { get; init; }

    protected IServiceProvider Services { get; init; }

    protected HttpClient AppHttpClient { get; init; }

    protected TestData TestData { get; init; }

    protected async Task ExecuteInDb(Func<FileServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        FileServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        await action(dbContext);
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
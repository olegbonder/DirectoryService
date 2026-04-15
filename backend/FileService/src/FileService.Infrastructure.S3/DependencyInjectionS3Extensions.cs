using Amazon.S3;
using FileService.Core.FilesStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public static class DependencyInjectionS3Extensions
{
    public static IServiceCollection AddS3(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(configuration.GetSection(nameof(FileStorageOptions)));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            FileStorageOptions s3Options = sp.GetRequiredService<IOptions<FileStorageOptions>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = s3Options.EndPoint, UseHttp = !s3Options.WithSsl, ForcePathStyle = true
            };

            return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
        });

        services.AddScoped<IFileStorageProvider, FileStorageProvider>();

        services.AddHostedService<S3BucketInitializationService>();

        services.AddTransient<IChunkSizeCalculator, ChunkSizeCalculator>();

        return services;
    }
}
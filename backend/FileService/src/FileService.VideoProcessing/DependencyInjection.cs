using FileService.Core;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace FileService.VideoProcessing;

public static class DependencyInjection
{
    public static IServiceCollection AddVideoProcessing(this IServiceCollection services)
    {
        services.AddQuartz(configure =>
        {
            configure.UseSimpleTypeLoader();
            configure.UseInMemoryStore();

            configure.AddJob<VideoProcessingJob>(options =>
            {
                options.WithIdentity(new JobKey(nameof(VideoProcessingJob)));
                options.StoreDurably();
            });
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.AddScoped<IVideoProcessingScheduler, VideoProcessingScheduler>();

        return services;
    }
}

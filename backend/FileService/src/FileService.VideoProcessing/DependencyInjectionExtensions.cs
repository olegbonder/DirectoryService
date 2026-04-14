using FileService.Core;
using FileService.VideoProcessing.BackgroundServices;
using FileService.VideoProcessing.FfmpegProcess;
using FileService.VideoProcessing.Pipeline;
using FileService.VideoProcessing.Pipeline.Steps;
using FileService.VideoProcessing.ProcessRunner;
using FileService.VideoProcessing.Progress;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace FileService.VideoProcessing;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddVideoProcessing(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<VideoProcessingOptions>(
            configuration.GetSection(VideoProcessingOptions.SECTION_NAME));

        services.AddScoped<IProcessRunner, ProcessRunner.ProcessRunner>();
        services.AddScoped<IFfmpegProcessRunner, FfmpegProcessRunner>();

        services.AddScoped<IVideoProcessingService, VideoProcessingService>();
        services.AddScoped<IVideoProgressReporter, VideoProgressReporter>();
        services.AddScoped<IProcessingPipeline, ProcessingPipeline>();
        services.AddSingleton<IProgressEventQueue, InMemoryProgressQueue>();

        services.AddScoped<IProcessingStepHandler, InitializeStepHandler>();
        services.AddScoped<IProcessingStepHandler, ExtractMetadataStepHandler>();
        services.AddScoped<IProcessingStepHandler, GenerateHlsStepHandler>();
        services.AddScoped<IProcessingStepHandler, UploadeHlsStepHandler>();
        services.AddScoped<IProcessingStepHandler, CleanupStepHandler>();

        services.AddHostedService<ProgressConsumer>();

        return services;
    }

    public static IServiceCollection AddQuartzService(this IServiceCollection services)
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
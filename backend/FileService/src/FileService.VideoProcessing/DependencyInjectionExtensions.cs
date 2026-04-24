using FileService.Core;
using FileService.Core.Processing;
using FileService.VideoProcessing.FfmpegProcess;
using FileService.VideoProcessing.Jobs;
using FileService.VideoProcessing.Pipeline;
using FileService.VideoProcessing.Pipeline.Steps;
using FileService.VideoProcessing.ProcessRunner;
using FileService.VideoProcessing.Progress;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<IProcessingJobFactory, VideoProcessingJobFactory>();

        services.AddHostedService<ProgressConsumer>();

        return services;
    }
}
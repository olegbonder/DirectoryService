using FileService.Core;
using FileService.Presenters.Notifiers;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Presenters;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProgressNotifier(this IServiceCollection services)
    {
        services.AddSingleton<IProgressNotifier, SignalRProgressNotifier>();

        return services;
    }
}
using Core.Abstractions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application;

public static class DependencyInjectionCore
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjectionCore).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        services.AddHandlers(assembly);
        return services;
    }
}
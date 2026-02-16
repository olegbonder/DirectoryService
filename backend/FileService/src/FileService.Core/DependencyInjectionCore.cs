using Core.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Core
{
    public static class DependencyInjectionCore
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjectionCore).Assembly;
            services.AddValidatorsFromAssembly(assembly);

            services.AddHandlers(assembly);

            return services;
        }
    }
}

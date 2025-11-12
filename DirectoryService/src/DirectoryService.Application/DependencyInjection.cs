using DirectoryService.Application.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;
            services.AddValidatorsFromAssembly(assembly);

            // Поищет в текущей сборке все реализации для ICommandHandler
            services.Scan(scan => scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}

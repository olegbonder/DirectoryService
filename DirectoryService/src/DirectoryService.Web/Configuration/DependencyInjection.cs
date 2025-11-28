using DirectoryService.Application;
using DirectoryService.Application.Features.Locations.Commands.CreateLocation;
using DirectoryService.Presenters.EndpointResult;
using Serilog;
using Serilog.Exceptions;
using Shared.Result;

namespace DirectoryService.Web.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSerilogLogging(configuration)
                .AddWebDependencies()
                .AddApplication();

            return services;
        }

        private static IServiceCollection AddWebDependencies(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddOpenApi(options =>
            {
                options.AddSchemaTransformer((schema, context, _) =>
                {
                    if (context.JsonTypeInfo.Type == typeof(Envelope<Error>))
                    {
                        if (schema.Properties.TryGetValue("error", out var errorsProp))
                        {
                            errorsProp.Items.Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.Schema,
                                Id = "Error",
                            };
                        }
                    }

                    return Task.CompletedTask;
                });
            });

            return services;
        }

        private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSerilog((serviceProvider, lo) => lo
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(serviceProvider)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("HandlerName", nameof(CreateLocationHandler)));
            return services;
        }
    }
}

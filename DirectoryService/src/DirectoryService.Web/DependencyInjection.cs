using DirectoryService.Application;
using DirectoryService.Presenters.EndpointResult;
using Shared.Result;

namespace DirectoryService.Web
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProgramDependencies(this IServiceCollection services)
        {
            services
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
    }
}

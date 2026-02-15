using Microsoft.AspNetCore.Builder;

namespace Framework.Cors;

public static class CorsExtensions
{
    public static IApplicationBuilder ConfigureCors(this IApplicationBuilder app, string origin)
    {
        app.UseCors(builder =>
        {
            builder
                .WithOrigins(origin)
                .AllowAnyMethod()
                .AllowAnyHeader();
        });

        return app;
    }
}
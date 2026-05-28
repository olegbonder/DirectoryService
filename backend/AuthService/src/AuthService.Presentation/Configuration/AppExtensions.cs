using CrystalQuartz.AspNetCore;
using Framework.Cors;
using Framework.Endpoints;
using Framework.Middlewares;
using Framework.Swagger;
using Quartz;
using Serilog;
using SharedAuth.Jwt;

namespace AuthService.Presentation.Configuration
{
    public static class AppExtensions
    {
        public static IApplicationBuilder ConfigureApp(this WebApplication app)
        {
            app.UseSerilogRequestLogging();
            app.UseExceptionMiddleware();

            if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
            {
                app.ConfigureOpenApiSpec("/openapi/v1.json", "AuthService.Web");
            }

            var apiGroup = app.MapGroup("/api").WithOpenApi();

            app.MapEndpoints(apiGroup);

            app.ConfigureCors("http://localhost:3000");

            app.UseRouting();
            app.UseCrystalQuartz(() =>
            {
                var factory = app.Services.GetRequiredService<ISchedulerFactory>();
                return factory.GetScheduler().GetAwaiter().GetResult();
            });

            app.UseJwtAuthentication();

            return app;
        }
    }
}

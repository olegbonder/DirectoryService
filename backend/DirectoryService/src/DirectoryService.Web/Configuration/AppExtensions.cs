using Framework.Cors;
using Framework.Logging;
using Framework.Middlewares;
using Framework.Swagger;
using Serilog;

namespace DirectoryService.Web.Configuration
{
    public static class AppExtensions
    {
        public static IApplicationBuilder ConfigureApp(this WebApplication app)
        {
            app.UseSerilogRequestLogging();
            app.UseExceptionMiddleware();

            if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
            {
                app.ConfigureOpenApiSpec("/openapi/v1.json", "DirectoryService.Web");
            }

            app.MapControllers();

            app.ConfigureCors("http://localhost:3000");

            return app;
        }
    }
}

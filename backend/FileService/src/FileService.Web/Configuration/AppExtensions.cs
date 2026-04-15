using FileService.Presenters.SignalRExtensions;
using FileService.VideoProcessing.Progress;
using Framework.Cors;
using Framework.Endpoints;
using Framework.Middlewares;
using Framework.Swagger;
using Serilog;

namespace FileService.Web.Configuration
{
    public static class AppExtensions
    {
        public static IApplicationBuilder ConfigureApp(this WebApplication app)
        {
            app.UseSerilogRequestLogging();
            app.UseExceptionMiddleware();

            if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
            {
                app.ConfigureOpenApiSpec("/openapi/v1.json", "FileService.Web");
            }

            var apiGroup = app.MapGroup("/api").WithOpenApi();

            app.MapEndpoints(apiGroup);

            app.UseSignalR();

            app.ConfigureCors("http://localhost:3000");

            return app;
        }
    }
}

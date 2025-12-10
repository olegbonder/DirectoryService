using DirectoryService.Web.Middlewares;
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
                app.MapOpenApi();
                app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService.Web"));
            }

            app.MapControllers();

            return app;
        }
    }
}

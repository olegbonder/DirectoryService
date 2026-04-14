using FileService.Presenters.Hubs;
using Microsoft.AspNetCore.Builder;

namespace FileService.Presenters.SignalRExtensions;

public static class SignalRExtensions
{
    public static IApplicationBuilder UseSignalR(this WebApplication app)
    {
        app.MapHub<ProgressHub>("/progressHub");

        return app;
    }
}
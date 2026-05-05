using AuthService.Application;
using AuthService.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddDbContext(configuration)
                .AddIdentity();

            return services;
        }

        private static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                })
                .AddEntityFrameworkStores<AuthDbContext>();

            return services;
        }

        private static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextPool<AuthDbContext>((sp, options) =>
            {
                string? connectionString = configuration.GetConnectionString(ConnectionStringNames.DATABASE);
                var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                options.UseNpgsql(connectionString);

                if (hostEnvironment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }

                options.UseLoggerFactory(loggerFactory);
            });
            /*services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(connectionString)
                    .UseLoggerFactory(CreateLoggerFactory())
                    .EnableSensitiveDataLogging());*/
            /*string? connectionString = configuration.GetConnectionString(ConnectionStringNames.DATABASE);
            services.AddScoped(s => new AuthDbContext(connectionString!));*/

            return services;
        }
    }
}

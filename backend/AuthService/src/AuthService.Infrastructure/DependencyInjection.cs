using AuthService.Application;
using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Infrastructure.Database;
using AuthService.Infrastructure.Jwt;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Seed;
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
                .AddIdentity()
                .AddIdentitySeeding(configuration)
                .AddJwtAuthentication(configuration);

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
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        private static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITransactionManager, TransactionManager>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

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

            return services;
        }

        private static IServiceCollection AddIdentitySeeding(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AdminOptions>(configuration.GetSection(AdminOptions.SECTION_NAME));
            services.AddHostedService<RolesInitializationService>();

            return services;
        }

        private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SECTION_NAME));
            services.AddScoped<ITokenProvider, TokenProvider>();
            services.AddAuthentication()
                .AddJwtBearer();

            return services;
        }
    }
}

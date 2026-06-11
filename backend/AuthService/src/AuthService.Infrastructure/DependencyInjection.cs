using AuthService.Application;
using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Infrastructure.Database;
using AuthService.Infrastructure.EmailSender;
using AuthService.Infrastructure.Jobs;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Seed;
using AuthService.Infrastructure.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedAuth.DevAuth;
using SharedAuth.Jwt;
using SharedAuth.Permissions;

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
                .AddTokenProvider()
                .AddJwtAuthentication(configuration)
                .AddEmail(configuration)
                .AddPermissionAuthorization()
                .AddDevAuth(configuration)
                .AddQuartzServices(configuration)
                .AddCookie();

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

            services.AddDbContextPool<IReadDbContext, AuthDbContext>((sp, options) =>
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

        private static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailOptions>(configuration.GetSection(MailOptions.SECTION_NAME));
            services.AddScoped<IEmailSender, EmailSender.EmailSender>();

            return services;
        }

        private static IServiceCollection AddTokenProvider(this IServiceCollection services)
        {
            services.AddScoped<ITokenProvider, TokenProvider>();

            return services;
        }

        private static IServiceCollection AddQuartzServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<CleanUpRefreshTokenJobOptions>(configuration.GetSection(CleanUpRefreshTokenJobOptions.SECTION_NAME));

            var jobOptions = configuration
                .GetSection(CleanUpRefreshTokenJobOptions.SECTION_NAME)
                .Get<CleanUpRefreshTokenJobOptions>() ?? new CleanUpRefreshTokenJobOptions();
            services.AddQuartz(options =>
            {
                /*options.UsePersistentStore(persistenceOptions =>
                {
                    persistenceOptions.UsePostgres(cfg =>
                    {
                        cfg.ConnectionString = configuration.GetConnectionString(ConnectionStringNames.DATABASE)!;
                    });

                    persistenceOptions.UseNewtonsoftJsonSerializer();
                    persistenceOptions.UseProperties = true;
                });*/

                var jobKey = new JobKey("clean-up-refresh-token");
                options.AddJob<CleanUpRefreshTokenJob>(opts =>
                    opts.WithIdentity(jobKey)
                        .StoreDurably(true) // job not delete if not triggers
                        .RequestRecovery(true)); // job retry if application failed

                options.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("clean-up-refresh-token-trigger")
                    .WithSchedule(SimpleScheduleBuilder.Create()
                        .WithInterval(TimeSpan.FromDays(jobOptions.IntervalInDays))
                        .RepeatForever()));
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            return services;
        }

        private static IServiceCollection AddCookie(this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Path = RefreshTokenCookieManager.COOKIE_PATH;
            });

            services.AddSingleton<IRefreshTokenCookieManager, RefreshTokenCookieManager>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}

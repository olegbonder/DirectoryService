using IntegrationEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace DirectoryService.Application.Messaging;

public static class WolverineConfiguration
{
    public static void AddWolverine(this WebApplicationBuilder builder)
    {
        string rabbitMQConnectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.RABBIT_MQ)!;
        string postgresConnectionString = builder.Configuration.GetConnectionString(ConnectionStringNames.DATABASE)!;

        builder.Host.UseWolverine(
            opts =>
        {
            opts.ApplicationAssembly = typeof(WolverineConfiguration).Assembly;

            opts.ConfigureDurableMessaging(postgresConnectionString);
            opts.ConfigureRabbitMq(rabbitMQConnectionString);
            opts.ConfigureStandardErrorPolicies();
        }, ExtensionDiscovery.ManualOnly);
    }

    private static void ConfigureDurableMessaging(this WolverineOptions opts, string postgresConnectionString)
    {
        opts.PersistMessagesWithPostgresql(postgresConnectionString, "public");
        opts.UseEntityFrameworkCoreTransactions();
        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    }
}
using IntegrationEvents;
using IntegrationEvents.Directory;
using IntegrationEvents.Directory.Events;
using Wolverine;
using Wolverine.RabbitMQ;

namespace DirectoryService.Application.Messaging;

public static class RabbitMqConfiguration
{
    private const string DIRECTORY_DEPARTMENT_FILE_EVENTS_QUEUE = "directory.department-file-events";

    public static void ConfigureRabbitMq(this WolverineOptions options, string connectionString)
    {
        options.UseRabbitMq(new Uri(connectionString))
            .AutoProvision()
            .EnableWolverineControlQueues()
            .UseQuorumQueues()
            .DeclareExchange(DirectoryEventsRouting.EXCHANGE, exchange =>
            {
                exchange.ExchangeType = ExchangeType.Fanout;
                exchange.IsDurable = true;
            })
            .DeclareExchange(FileEventsRouting.EXCHANGE, exchange =>
            {
                exchange.ExchangeType = ExchangeType.Topic;
                exchange.IsDurable = true;
            });

        options.ConfigureDirectoryEventsPublishing();
        options.ConfigureDirectoryEventsListeners();
    }

    private static void ConfigureDirectoryEventsPublishing(this WolverineOptions opts)
    {
        /*opts.PublishMessagesToRabbitMqExchange<DepartmentDeleted>(
            DirectoryEventsRouting.EXCHANGE,
            n => DirectoryEventsRouting.RoutingKeys.DepartmentDeleted(n.Context)).UseDurableOutbox();*/
        string exchange = DirectoryEventsRouting.EXCHANGE;

        opts.PublishMessage<DepartmentDeleted>().ToRabbitQueue(exchange).UseDurableOutbox();
    }

    private static void ConfigureDirectoryEventsListeners(this WolverineOptions opts)
    {
        opts.ListenToRabbitQueue(DIRECTORY_DEPARTMENT_FILE_EVENTS_QUEUE, queue =>
        {
            queue.BindExchange(FileEventsRouting.EXCHANGE, FileEventsRouting.RoutingKeys.ALL_DEPARTMENT_EVENTS);
        });
    }
}
using IntegrationEvents;
using IntegrationEvents.Directory;
using IntegrationEvents.Files.Events;
using Wolverine;
using Wolverine.RabbitMQ;

namespace FileService.Core.Messaging;

public static class RabbitMqConfiguration
{
    private const string FILE_HARD_DELETES_QUEUE = "file.hard-delete";
    private const string FILES_HARD_DELETES_QUEUE = "files.hard-delete";

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

        options.ConfigureFileEventsPublishing();
        options.ConfigureEducationEventsListeners();
    }

    private static void ConfigureEducationEventsListeners(this WolverineOptions opts)
    {
        opts.ListenToRabbitQueue(FILE_HARD_DELETES_QUEUE, queue =>
        {
            queue.BindExchange(DirectoryEventsRouting.EXCHANGE);
        });
        opts.ListenToRabbitQueue(FILES_HARD_DELETES_QUEUE, queue =>
        {
            queue.BindExchange(DirectoryEventsRouting.EXCHANGE);
        });
    }

    private static void ConfigureFileEventsPublishing(this WolverineOptions opts)
    {
        opts.PublishMessagesToRabbitMqExchange<VideoCreated>(
            FileEventsRouting.EXCHANGE,
            n => FileEventsRouting.RoutingKeys.VideoCreated(n.EntityType)).UseDurableOutbox();

        opts.PublishMessagesToRabbitMqExchange<VideoDeleted>(
            FileEventsRouting.EXCHANGE,
            n => FileEventsRouting.RoutingKeys.VideoDeleted(n.EntityType)).UseDurableOutbox();

        opts.PublishMessagesToRabbitMqExchange<PreviewCreated>(
            FileEventsRouting.EXCHANGE,
            n => FileEventsRouting.RoutingKeys.PreviewCreated(n.EntityType)).UseDurableOutbox();

        opts.PublishMessagesToRabbitMqExchange<PreviewDeleted>(
            FileEventsRouting.EXCHANGE,
            n => FileEventsRouting.RoutingKeys.PreviewDeleted(n.EntityType)).UseDurableOutbox();
    }
}
using FileService.Core.Database;
using Wolverine.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class OutboxService : IOutboxService
{
    private readonly IDbContextOutbox<FileServiceDbContext> _outbox;

    public OutboxService(IDbContextOutbox<FileServiceDbContext> outbox)
    {
        _outbox = outbox;
    }

    public async Task PublishAsync<T>(T message)
        where T : class => await _outbox.PublishAsync(message);

    public Task FlushAsync() => _outbox.FlushOutgoingMessagesAsync();
}
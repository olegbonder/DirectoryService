using DirectoryService.Application.Abstractions.Database;
using Wolverine.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Database;

public class OutboxService : IOutboxService
{
    private readonly IDbContextOutbox<ApplicationDbContext> _outbox;

    public OutboxService(IDbContextOutbox<ApplicationDbContext> outbox)
    {
        _outbox = outbox;
    }

    public async Task PublishAsync<T>(T message)
        where T : class => await _outbox.PublishAsync(message);

    public Task FlushAsync() => _outbox.FlushOutgoingMessagesAsync();
}
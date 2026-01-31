using POC.Application.Ports;
using POC.Infra.Persistence.Entities;

namespace POC.Infra.Persistence;

/// <summary>
/// Adapter: escreve na Outbox no mesmo DbContext (mesma transação que SaveChanges).
/// </summary>
public class OutboxWriter : IOutboxWriter
{
    private readonly AppDbContext _db;

    public OutboxWriter(AppDbContext db)
    {
        _db = db;
    }

    public Task WriteAsync(string type, Guid aggregateId, string payloadJson, DateTime occurredAt, CancellationToken ct = default)
    {
        _db.Outbox.Add(new OutboxEntity
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            Type = type,
            Payload = payloadJson,
            OccurredAt = occurredAt,
            Attempts = 0
        });
        return Task.CompletedTask;
    }
}

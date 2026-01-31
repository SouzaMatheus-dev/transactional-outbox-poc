using Microsoft.EntityFrameworkCore;
using POC.Application.Ports;
using POC.Infra.Persistence.Entities;

namespace POC.Infra.Persistence;

/// <summary>
/// Adapter: persiste eventos recebidos (Consumer) para auditoria.
/// </summary>
public class ReceivedEventStore : IReceivedEventStore
{
    private readonly AppDbContext _db;

    public ReceivedEventStore(AppDbContext db)
    {
        _db = db;
    }

    public async Task StoreAsync(string messageKey, string topic, CancellationToken ct = default)
    {
        _db.ReceivedEvents.Add(new ReceivedEventEntity
        {
            Id = Guid.NewGuid(),
            MessageKey = messageKey,
            Topic = topic,
            ReceivedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}

using Microsoft.EntityFrameworkCore;
using POC.Application.Ports;
using POC.Infra.Persistence.Entities;

namespace POC.Infra.Persistence;

/// <summary>
/// Adapter: lê pendentes da Outbox, aplica lock, marca processado/falha.
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly AppDbContext _db;

    public OutboxRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int batchSize, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var items = await _db.Outbox
            .Where(x => x.ProcessedAt == null && (x.LockedUntil == null || x.LockedUntil < now))
            .OrderBy(x => x.OccurredAt)
            .Take(batchSize)
            .Select(x => new OutboxEntry(x.Id, x.AggregateId, x.Type, x.Payload, x.OccurredAt, x.Attempts))
            .ToListAsync(ct);
        return items;
    }

    public async Task<bool> TryLockAsync(Guid outboxId, Guid lockId, TimeSpan lockDuration, CancellationToken ct = default)
    {
        var lockedUntil = DateTime.UtcNow.Add(lockDuration);
        var updated = await _db.Outbox
            .Where(x => x.Id == outboxId && x.ProcessedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.LockedUntil, lockedUntil)
                .SetProperty(x => x.LockId, lockId), ct);
        return updated > 0;
    }

    public async Task MarkProcessedAsync(Guid outboxId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        await _db.Outbox
            .Where(x => x.Id == outboxId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ProcessedAt, now)
                .SetProperty(x => x.LockedUntil, (DateTime?)null)
                .SetProperty(x => x.LockId, (Guid?)null), ct);
    }

    public async Task MarkFailedAsync(Guid outboxId, string lastError, CancellationToken ct = default)
    {
        var entity = await _db.Outbox.AsNoTracking().FirstOrDefaultAsync(x => x.Id == outboxId, ct);
        if (entity == null) return;
        var newAttempts = entity.Attempts + 1;
        await _db.Outbox
            .Where(x => x.Id == outboxId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Attempts, newAttempts)
                .SetProperty(x => x.LastError, lastError)
                .SetProperty(x => x.LockedUntil, (DateTime?)null)
                .SetProperty(x => x.LockId, (Guid?)null), ct);
    }
}

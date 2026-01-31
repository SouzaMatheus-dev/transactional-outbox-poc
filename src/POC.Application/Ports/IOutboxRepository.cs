namespace POC.Application.Ports;

/// <summary>
/// Porta: repositório da Outbox (leitura de pendentes e atualização de ProcessedAt/Lock).
/// A implementação fica na Infra.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Busca N registros não processados e não bloqueados (ou com lock expirado).
    /// </summary>
    Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int batchSize, CancellationToken ct = default);

    /// <summary>
    /// Aplica lock otimista: atualiza LockedUntil e LockId.
    /// </summary>
    Task<bool> TryLockAsync(Guid outboxId, Guid lockId, TimeSpan lockDuration, CancellationToken ct = default);

    /// <summary>
    /// Marca como processado (ProcessedAt = now).
    /// </summary>
    Task MarkProcessedAsync(Guid outboxId, CancellationToken ct = default);

    /// <summary>
    /// Em falha: incrementa Attempts, seta LastError e libera lock.
    /// </summary>
    Task MarkFailedAsync(Guid outboxId, string lastError, CancellationToken ct = default);
}

/// <summary>
/// DTO lido da Outbox para o Dispatcher publicar.
/// </summary>
public record OutboxEntry(
    Guid Id,
    Guid AggregateId,
    string Type,
    string Payload,
    DateTime OccurredAt,
    int Attempts
);

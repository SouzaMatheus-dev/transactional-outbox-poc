namespace POC.Infra.Persistence.Entities;

/// <summary>
/// Entidade de persistência da tabela Outbox (adapter).
/// </summary>
public class OutboxEntity
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public Guid? LockId { get; set; }
    public string? LastError { get; set; }
}

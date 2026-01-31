namespace POC.Domain.Events;

/// <summary>
/// Evento de domínio: Inicialização criada.
/// Será serializado no Payload da Outbox e publicado no Kafka.
/// </summary>
public record InitializationCreatedEvent
{
    public Guid AggregateId { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public int Status { get; init; }
    public DateTime OccurredAt { get; init; }
    /// <summary>
    /// CorrelationId para rastrear o fluxo entre API, Dispatcher e Consumer.
    /// </summary>
    public string? CorrelationId { get; init; }
}

namespace POC.Application.Ports;

/// <summary>
/// Porta: escrita na Outbox (usada no mesmo contexto de transação que a entidade de domínio).
/// A implementação na Infra garante que Insert na Outbox ocorre na mesma transação do SaveChanges.
/// </summary>
public interface IOutboxWriter
{
    Task WriteAsync(string type, Guid aggregateId, string payloadJson, DateTime occurredAt, CancellationToken ct = default);
}

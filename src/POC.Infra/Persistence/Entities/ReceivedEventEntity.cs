namespace POC.Infra.Persistence.Entities;

/// <summary>
/// Entidade opcional: eventos recebidos pelo Consumer (auditoria).
/// </summary>
public class ReceivedEventEntity
{
    public Guid Id { get; set; }
    public string MessageKey { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
}

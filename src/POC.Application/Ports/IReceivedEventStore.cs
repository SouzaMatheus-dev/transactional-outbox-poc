namespace POC.Application.Ports;

/// <summary>
/// Porta opcional: persistir eventos recebidos pelo Consumer (auditoria).
/// </summary>
public interface IReceivedEventStore
{
    Task StoreAsync(string messageKey, string topic, CancellationToken ct = default);
}

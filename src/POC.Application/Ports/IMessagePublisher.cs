namespace POC.Application.Ports;

/// <summary>
/// Porta: publicação de mensagens no barramento (Kafka).
/// O Dispatcher usa isso para publicar após ler da Outbox.
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync(string topic, string key, string value, CancellationToken ct = default);
}

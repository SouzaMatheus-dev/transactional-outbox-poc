using Confluent.Kafka;
using Microsoft.Extensions.Options;
using POC.Application.Ports;

namespace POC.Infra.Kafka;

/// <summary>
/// Adapter: publica mensagens no Kafka (Producer).
/// Usado pelo Worker.Dispatcher após ler da Outbox.
/// </summary>
public class KafkaProducer : IMessagePublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;

    public KafkaProducer(IOptions<KafkaOptions> options)
    {
        _options = options.Value;
        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = _options.ProducerIdempotence,
            MessageSendMaxRetries = 3
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(string topic, string key, string value, CancellationToken ct = default)
    {
        var message = new Message<string, string> { Key = key, Value = value };
        await _producer.ProduceAsync(topic, message, ct);
    }

    public void Dispose() => _producer.Dispose();
}

namespace POC.Infra.Kafka;

/// <summary>
/// Configuração do Kafka (bootstrap, tópicos, producer/consumer).
/// </summary>
public class KafkaOptions
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string TopicInitializationCreated { get; set; } = "poc.initialization.created";
    public string ConsumerGroup { get; set; } = "poc-consumer";
    /// <summary>
    /// Producer: acks=all para durabilidade; enable.idempotence quando suportado.
    /// </summary>
    public bool ProducerIdempotence { get; set; } = true;
}

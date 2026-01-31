using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using POC.Application.Ports;
using POC.Infra.Kafka;

namespace POC.Worker.Consumer;

/// <summary>
/// Worker: consome mensagens do tópico poc.initialization.created, loga e opcionalmente persiste em ReceivedEvents.
/// </summary>
public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(IServiceScopeFactory scopeFactory, IOptions<KafkaOptions> options, ILogger<KafkaConsumerService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_options.TopicInitializationCreated);
        _logger.LogInformation("Consumer inscrito no tópico {Topic}, GroupId={GroupId}", _options.TopicInitializationCreated, _options.ConsumerGroup);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));
                    if (consumeResult == null) continue;

                    var key = consumeResult.Message.Key ?? "";
                    var value = consumeResult.Message.Value ?? "";
                    _logger.LogInformation("Consumer received - Topic={Topic}, Key={Key}, CorrelationId (no payload)", consumeResult.Topic, key);

                    // Log do payload (extrair CorrelationId se existir)
                    string? correlationId = null;
                    try
                    {
                        using var doc = JsonDocument.Parse(value);
                        if (doc.RootElement.TryGetProperty("CorrelationId", out var cid))
                            correlationId = cid.GetString();
                    }
                    catch { /* ignore */ }
                    _logger.LogInformation("Consumer processando - Key={Key}, CorrelationId={CorrelationId}, ReceivedAt={ReceivedAt}", key, correlationId ?? "-", DateTime.UtcNow);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var store = scope.ServiceProvider.GetService<IReceivedEventStore>();
                        if (store != null)
                        {
                            await store.StoreAsync(key, consumeResult.Topic, stoppingToken);
                            _logger.LogInformation("Consumer persistiu ReceivedEvent - Key={Key}", key);
                        }
                    }

                    consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    // Tópico ainda não existe (criado na 1ª publicação ou pelo kafka-create-topic no docker-compose)
                    if (ex.Error.Reason?.Contains("Unknown topic or partition", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        _logger.LogWarning("Tópico {Topic} ainda não existe no broker; aguardando 10s antes de tentar novamente. Crie o tópico ou envie um POST /initializations para o Dispatcher publicar.", _options.TopicInitializationCreated);
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                    else
                    {
                        _logger.LogError(ex, "Erro ao consumir mensagem");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        finally
        {
            consumer.Close();
        }

        await Task.CompletedTask;
    }
}

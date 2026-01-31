using Microsoft.Extensions.Options;
using POC.Application.Ports;
using POC.Infra.Kafka;

namespace POC.Worker.Dispatcher;

/// <summary>
/// Worker: lê a Outbox em lote, publica no Kafka e marca como processado (ou falha com retry).
/// Lock otimista para evitar processamento duplicado entre pods.
/// </summary>
public class OutboxDispatcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessagePublisher _publisher;
    private readonly KafkaOptions _kafkaOptions;
    private readonly DispatcherOptions _options;
    private readonly ILogger<OutboxDispatcherService> _logger;

    public OutboxDispatcherService(
        IServiceScopeFactory scopeFactory,
        IMessagePublisher publisher,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<DispatcherOptions> options,
        ILogger<OutboxDispatcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _kafkaOptions = kafkaOptions.Value;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dispatcher iniciado - PollInterval={PollInterval}s, BatchSize={BatchSize}",
            _options.PollIntervalSeconds, _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no ciclo do Dispatcher; continuando no próximo poll.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        var pending = await outboxRepo.GetPendingAsync(_options.BatchSize, ct);
        if (pending.Count == 0)
            return;

        _logger.LogInformation("Dispatcher processando lote de {Count} registros da Outbox", pending.Count);
        var lockDuration = TimeSpan.FromSeconds(_options.LockDurationSeconds);
        var topic = _kafkaOptions.TopicInitializationCreated;

        foreach (var entry in pending)
        {
            var lockId = Guid.NewGuid();
            try
            {
                var locked = await outboxRepo.TryLockAsync(entry.Id, lockId, lockDuration, ct);
                if (!locked)
                {
                    _logger.LogDebug("Registro Outbox {Id} não foi lockado (outro pod pode ter pegado)", entry.Id);
                    continue;
                }

                _logger.LogInformation("Dispatcher publicando Outbox Id={Id}, AggregateId={AggregateId}, Type={Type}, CorrelationId será no Payload",
                    entry.Id, entry.AggregateId, entry.Type);

                await _publisher.PublishAsync(topic, entry.AggregateId.ToString(), entry.Payload, ct);
                await outboxRepo.MarkProcessedAsync(entry.Id, ct);
                _logger.LogInformation("Dispatcher marcou Outbox Id={Id} como ProcessedAt", entry.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar Outbox Id={Id}; marcando falha e liberando lock", entry.Id);
                try
                {
                    await outboxRepo.MarkFailedAsync(entry.Id, ex.Message, ct);
                }
                catch (Exception inner)
                {
                    _logger.LogError(inner, "Erro ao chamar MarkFailedAsync para Outbox Id={Id}", entry.Id);
                }
            }
        }
    }
}

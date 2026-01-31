using System.Text.Json;
using POC.Application.Commands;
using POC.Application.Ports;
using POC.Domain.Entities;
using POC.Domain.Events;

namespace POC.Application.Handlers;

/// <summary>
/// Use case: criar Initialization e registrar evento na Outbox na mesma transação.
/// </summary>
public class CreateInitializationHandler
{
    private readonly IInitializationRepository _initializationRepository;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;

    public const string EventType = "InitializationCreated";

    public CreateInitializationHandler(
        IInitializationRepository initializationRepository,
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork)
    {
        _initializationRepository = initializationRepository;
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateInitializationResult> HandleAsync(CreateInitializationCommand command, CancellationToken ct = default)
    {
        var entity = Initialization.Create(command.ExternalId);
        await _initializationRepository.AddAsync(entity, ct);

        var domainEvent = new InitializationCreatedEvent
        {
            AggregateId = entity.Id,
            ExternalId = entity.ExternalId,
            Status = (int)entity.Status,
            OccurredAt = entity.CreatedAt,
            CorrelationId = command.CorrelationId
        };
        var payloadJson = JsonSerializer.Serialize(domainEvent);
        await _outboxWriter.WriteAsync(EventType, entity.Id, payloadJson, entity.CreatedAt, ct);

        // Commit único: Initialization + Outbox na mesma transação
        await _unitOfWork.SaveChangesAsync(ct);

        return new CreateInitializationResult(entity.Id, entity.ExternalId, entity.CreatedAt);
    }
}

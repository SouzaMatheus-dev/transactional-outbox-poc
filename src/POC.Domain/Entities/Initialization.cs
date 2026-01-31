namespace POC.Domain.Entities;

/// <summary>
/// Aggregate raiz: Inicialização.
/// Regras de domínio e identidade ficam aqui.
/// </summary>
public class Initialization
{
    public Guid Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public InitializationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Initialization() { } // EF Core

    public static Initialization Create(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("ExternalId é obrigatório.", nameof(externalId));

        return new Initialization
        {
            Id = Guid.NewGuid(),
            ExternalId = externalId.Trim(),
            Status = InitializationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Status da inicialização (valor numérico para persistência).
/// </summary>
public enum InitializationStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}

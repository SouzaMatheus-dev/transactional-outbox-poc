namespace POC.Application.Commands;

/// <summary>
/// Resultado do use case CreateInitialization.
/// </summary>
public record CreateInitializationResult(Guid Id, string ExternalId, DateTime CreatedAt);

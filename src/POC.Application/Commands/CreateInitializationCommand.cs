namespace POC.Application.Commands;

/// <summary>
/// Command: criar uma nova Inicialização.
/// </summary>
public record CreateInitializationCommand(string ExternalId, string? CorrelationId = null);

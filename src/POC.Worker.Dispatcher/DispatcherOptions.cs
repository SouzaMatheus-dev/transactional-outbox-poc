namespace POC.Worker.Dispatcher;

/// <summary>
/// Configuração do Dispatcher: intervalo de poll e tamanho do lote.
/// </summary>
public class DispatcherOptions
{
    public const string SectionName = "Dispatcher";
    /// <summary>
    /// Intervalo em segundos entre cada leitura da Outbox.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 5;
    /// <summary>
    /// Quantidade máxima de registros por lote.
    /// </summary>
    public int BatchSize { get; set; } = 10;
    /// <summary>
    /// Duração do lock em segundos (LockedUntil = now + LockDurationSeconds).
    /// </summary>
    public int LockDurationSeconds { get; set; } = 60;
}

namespace POC.Application.Ports;

/// <summary>
/// Porta: unidade de trabalho para garantir commit da transação (Initialization + Outbox juntos).
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}

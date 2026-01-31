using POC.Domain.Entities;

namespace POC.Application.Ports;

/// <summary>
/// Porta (interface): repositório de Initialization.
/// A implementação fica na Infra (adapter).
/// </summary>
public interface IInitializationRepository
{
    Task<Initialization?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Initialization entity, CancellationToken ct = default);
}

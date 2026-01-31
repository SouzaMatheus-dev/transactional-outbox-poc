using Microsoft.EntityFrameworkCore;
using POC.Application.Ports;
using POC.Domain.Entities;
using POC.Infra.Persistence;

namespace POC.Infra.Persistence;

/// <summary>
/// Adapter: repositório de Initialization via EF Core.
/// </summary>
public class InitializationRepository : IInitializationRepository
{
    private readonly AppDbContext _db;

    public InitializationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Initialization?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Initializations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task AddAsync(Initialization entity, CancellationToken ct = default)
    {
        _db.Initializations.Add(entity);
        return Task.CompletedTask;
    }
}

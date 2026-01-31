using POC.Application.Ports;

namespace POC.Infra.Persistence;

/// <summary>
/// Adapter: Unit of Work = SaveChanges do DbContext (garante transação única).
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

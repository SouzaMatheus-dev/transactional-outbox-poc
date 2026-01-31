using Microsoft.EntityFrameworkCore;
using POC.Domain.Entities;
using POC.Infra.Persistence.Entities;

namespace POC.Infra.Persistence;

/// <summary>
/// DbContext: Initializations, Outbox e ReceivedEvents.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Initialization> Initializations => Set<Initialization>();
    public DbSet<OutboxEntity> Outbox => Set<OutboxEntity>();
    public DbSet<ReceivedEventEntity> ReceivedEvents => Set<ReceivedEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Initializations
        modelBuilder.Entity<Initialization>(e =>
        {
            e.ToTable("Initializations");
            e.HasKey(x => x.Id);
            e.Property(x => x.ExternalId).HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
        });

        // Outbox
        modelBuilder.Entity<OutboxEntity>(e =>
        {
            e.ToTable("Outbox");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(200);
            e.Property(x => x.Payload).HasColumnType("nvarchar(max)");
            e.Property(x => x.OccurredAt).HasColumnType("datetime2");
            e.Property(x => x.ProcessedAt).HasColumnType("datetime2");
            e.Property(x => x.LockedUntil).HasColumnType("datetime2");
            e.Property(x => x.LastError).HasColumnType("nvarchar(max)");
            e.HasIndex(x => new { x.ProcessedAt, x.LockedUntil });
        });

        // ReceivedEvents
        modelBuilder.Entity<ReceivedEventEntity>(e =>
        {
            e.ToTable("ReceivedEvents");
            e.HasKey(x => x.Id);
            e.Property(x => x.MessageKey).HasMaxLength(100);
            e.Property(x => x.Topic).HasMaxLength(200);
            e.Property(x => x.ReceivedAt).HasColumnType("datetime2");
        });
    }
}

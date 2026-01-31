using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POC.Application.Ports;
using POC.Infra.Kafka;
using POC.Infra.Persistence;

namespace POC.Infra;

/// <summary>
/// Registro dos adapters (Infra) no container de DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        // SQL + EF
        services.Configure<SqlOptions>(configuration.GetSection(SqlOptions.SectionName));
        var connectionString = configuration.GetConnectionString("Default") ?? configuration["Sql:ConnectionString"] ?? "";
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(3);
                sql.CommandTimeout(30);
            });
        });
        services.AddScoped<IInitializationRepository, InitializationRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReceivedEventStore, ReceivedEventStore>();

        // Kafka (Producer - singleton para reutilizar conexão)
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<IMessagePublisher, KafkaProducer>();

        return services;
    }

    /// <summary>
    /// Apenas Kafka (para Worker.Consumer que não usa SQL).
    /// </summary>
    public static IServiceCollection AddInfraKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        return services;
    }

    /// <summary>
    /// Apenas SQL + Outbox (para Worker.Dispatcher: lê Outbox, publica Kafka).
    /// </summary>
    public static IServiceCollection AddInfraDispatcher(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SqlOptions>(configuration.GetSection(SqlOptions.SectionName));
        var connectionString = configuration.GetConnectionString("Default") ?? configuration["Sql:ConnectionString"] ?? "";
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql => { sql.EnableRetryOnFailure(3); sql.CommandTimeout(30); }));
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<IMessagePublisher, KafkaProducer>();
        return services;
    }

    /// <summary>
    /// Para Worker.Consumer: Kafka consumer + opcionalmente ReceivedEventStore (SQL).
    /// </summary>
    public static IServiceCollection AddInfraConsumer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.Configure<SqlOptions>(configuration.GetSection(SqlOptions.SectionName));
        var connectionString = configuration.GetConnectionString("Default") ?? configuration["Sql:ConnectionString"] ?? "";
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql => { sql.EnableRetryOnFailure(3); sql.CommandTimeout(30); }));
        services.AddScoped<IReceivedEventStore, ReceivedEventStore>();
        return services;
    }
}

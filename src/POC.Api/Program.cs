using Microsoft.EntityFrameworkCore;
using POC.Api.Middleware;
using POC.Application.Handlers;
using POC.Infra;
using POC.Infra.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog estruturado com CorrelationId
builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "POC.Api")
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application: Handler
builder.Services.AddScoped<CreateInitializationHandler>();

// Infra: SQL + Outbox + Kafka (API só grava; não publica direto no Kafka)
builder.Services.AddInfra(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

// Swagger habilitado em todos os ambientes nesta POC (acessível via port-forward)
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

// Health check simples: GET /health retorna 200 se a API estiver de pé
app.MapGet("/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow })).AllowAnonymous();

// Garante que o banco POC existe e aplica migrations (POC; em producao use Job ou init container)
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("Default") ?? config["Sql:ConnectionString"] ?? "";
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            await POC.Infra.Persistence.SqlServerEnsureDatabase.EnsurePocDatabaseExistsAsync(connectionString);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Could not ensure database POC exists (e.g. SQL Server not ready). Migrations may fail.");
        }
    }
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

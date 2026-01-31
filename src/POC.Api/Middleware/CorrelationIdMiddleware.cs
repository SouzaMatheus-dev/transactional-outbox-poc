namespace POC.Api.Middleware;

/// <summary>
/// Gera ou propaga CorrelationId no request para rastrear o fluxo (API → Outbox → Dispatcher → Kafka → Consumer).
/// </summary>
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N")[..16];
        context.Response.Headers[HeaderName] = correlationId;
        context.TraceIdentifier = correlationId;
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

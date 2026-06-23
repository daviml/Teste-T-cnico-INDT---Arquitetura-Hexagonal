using Contracts;
using ContratacaoService.Infrastructure.Observability;
using Serilog.Context;

namespace ContratacaoService.API.Observability;

/// <summary>
/// Middleware de correlation id (driving adapter, concern transversal na borda).
/// Reaproveita o <c>X-Correlation-ID</c> recebido ou gera um novo; ecoa no
/// response, publica no <see cref="LogContext"/> (todo log da requisição passa a
/// carregá-lo) e no <see cref="CorrelationContext"/> (lido na publicação de eventos).
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, CorrelationContext correlation)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationHeader.Name, out var valor)
            && !string.IsNullOrWhiteSpace(valor)
                ? valor.ToString()
                : Guid.NewGuid().ToString();

        correlation.CorrelationId = correlationId;
        context.Response.Headers[CorrelationHeader.Name] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}

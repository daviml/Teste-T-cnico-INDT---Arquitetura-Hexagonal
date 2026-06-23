using Contracts;
using Serilog.Context;

namespace PropostaService.API.Observability;

/// <summary>
/// Middleware de correlation id (driving adapter, concern transversal na borda).
/// Reaproveita o <c>X-Correlation-ID</c> recebido ou gera um novo; ecoa no
/// response e publica no <see cref="LogContext"/> para que todo log da requisição
/// o carregue. (O PropostaService não publica eventos, então não há holder a
/// preencher — a correlação na fila chega pelo header da mensagem, ver consumer.)
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationHeader.Name, out var valor)
            && !string.IsNullOrWhiteSpace(valor)
                ? valor.ToString()
                : Guid.NewGuid().ToString();

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

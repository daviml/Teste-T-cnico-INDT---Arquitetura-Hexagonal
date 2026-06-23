using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ContratacaoService.API.Infrastructure;

/// <summary>
/// Rate limiting nativo na BORDA da API (driving adapter) — par <i>inbound</i> da
/// resiliência <i>outbound</i> (Polly no gateway). Concern transversal: não toca
/// domínio nem Application. Excesso responde <c>429</c> em ProblemDetails.
/// </summary>
internal static class RateLimitingExtensions
{
    /// <summary>Política aplicada aos endpoints de ESCRITA (POST/PATCH).</summary>
    public const string PoliticaEscrita = "escrita";

    public static IServiceCollection AddWriteRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Janela fixa por cliente (IP): orçamento de escrita curto e previsível.
            options.AddPolicy(PoliticaEscrita, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromSeconds(10),
                        QueueLimit = 0
                    }));

            // 429 em ProblemDetails (consistente com o resto da API) + Retry-After.
            options.OnRejected = async (context, _) =>
            {
                var http = context.HttpContext;
                http.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    http.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                var problemDetails = http.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetails.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = http,
                    ProblemDetails =
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Limite de requisições excedido",
                        Detail = "Muitas requisições em um curto período. Tente novamente em instantes."
                    }
                });
            };
        });

        return services;
    }
}

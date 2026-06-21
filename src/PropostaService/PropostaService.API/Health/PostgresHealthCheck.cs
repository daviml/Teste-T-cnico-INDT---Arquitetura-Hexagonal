using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PropostaService.Infrastructure.Persistence;

namespace PropostaService.API.Health;

/// <summary>Readiness: o serviço só está pronto se conseguir falar com o banco.</summary>
internal sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly PropostaDbContext _context;

    public PostgresHealthCheck(PropostaDbContext context) => _context = context;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
        => await _context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("PostgreSQL inacessível.");
}

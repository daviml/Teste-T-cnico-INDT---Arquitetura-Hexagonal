using ContratacaoService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ContratacaoService.API.Health;

/// <summary>Readiness: o serviço só está pronto se conseguir falar com o banco.</summary>
internal sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly ContratacaoDbContext _context;

    public PostgresHealthCheck(ContratacaoDbContext context) => _context = context;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
        => await _context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("PostgreSQL inacessível.");
}

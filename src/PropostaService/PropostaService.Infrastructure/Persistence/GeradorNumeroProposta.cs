using Microsoft.EntityFrameworkCore;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Infrastructure.Persistence;

internal sealed class GeradorNumeroProposta : IGeradorNumeroProposta
{
    // Identificador constante (sem entrada de usuário) → seguro contra injeção.
    private const string SqlProximoSequencial = "SELECT nextval('proposta_numero_seq') AS \"Value\"";

    private readonly PropostaDbContext _context;

    public GeradorNumeroProposta(PropostaDbContext context) => _context = context;

    public async Task<NumeroProposta> GerarProximoAsync(CancellationToken ct = default)
    {
        // Gaps são aceitáveis (a sequência não faz rollback); a unicidade é
        // garantida pelo índice único de NumeroProposta.
        var sequencial = await _context.Database
            .SqlQueryRaw<long>(SqlProximoSequencial)
            .SingleAsync(ct);

        return NumeroProposta.Gerar(DateTimeOffset.UtcNow.Year, sequencial);
    }
}

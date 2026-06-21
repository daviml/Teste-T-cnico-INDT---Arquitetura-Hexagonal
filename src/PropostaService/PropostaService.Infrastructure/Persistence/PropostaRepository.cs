using Microsoft.EntityFrameworkCore;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Domain.Common;
using PropostaService.Domain.Propostas;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Infrastructure.Persistence;

internal sealed class PropostaRepository : IPropostaRepository
{
    private readonly PropostaDbContext _context;

    public PropostaRepository(PropostaDbContext context) => _context = context;

    public Task AdicionarAsync(Proposta proposta, CancellationToken ct = default)
    {
        // Id é gerado no domínio; Add (síncrono) é suficiente.
        _context.Propostas.Add(proposta);
        return Task.CompletedTask;
    }

    public async Task<Proposta?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Propostas.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<Proposta> Itens, long Total)> ListarAsync(
        StatusProposta? status,
        string? documento,
        int pagina,
        int tamanhoPagina,
        CancellationToken ct = default)
    {
        var query = _context.Propostas.AsNoTracking();

        if (status is { } statusFiltro)
        {
            query = query.Where(p => p.Status == statusFiltro);
        }

        if (!string.IsNullOrWhiteSpace(documento))
        {
            // Busca por documento é match exato. Se o filtro não for um
            // documento válido, nenhum registro corresponde.
            var documentoFiltro = TentarCriarDocumento(documento);
            if (documentoFiltro is null)
            {
                return ([], 0);
            }

            query = query.Where(p => p.ClienteDocumento == documentoFiltro);
        }

        var total = await query.LongCountAsync(ct);

        var itens = await query
            .OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (itens, total);
    }

    private static Documento? TentarCriarDocumento(string documento)
    {
        try
        {
            return Documento.Criar(documento);
        }
        catch (DomainException)
        {
            return null;
        }
    }
}

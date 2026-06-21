using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.Contratacoes;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure.Persistence;

internal sealed class ContratacaoRepository : IContratacaoRepository
{
    private readonly ContratacaoDbContext _context;

    public ContratacaoRepository(ContratacaoDbContext context) => _context = context;

    public Task AdicionarAsync(Contratacao contratacao, CancellationToken ct = default)
    {
        _context.Contratacoes.Add(contratacao);
        return Task.CompletedTask;
    }

    public async Task<Contratacao?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Contratacoes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> ExisteParaPropostaAsync(Guid propostaId, CancellationToken ct = default)
        => await _context.Contratacoes.AnyAsync(c => c.PropostaId == propostaId, ct);
}

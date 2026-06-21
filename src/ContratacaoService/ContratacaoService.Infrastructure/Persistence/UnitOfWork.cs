using ContratacaoService.Application.Ports.Outbound;

namespace ContratacaoService.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly ContratacaoDbContext _context;

    public UnitOfWork(ContratacaoDbContext context) => _context = context;

    public Task<int> SalvarAlteracoesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}

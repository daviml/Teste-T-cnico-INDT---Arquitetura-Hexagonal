using PropostaService.Application.Ports.Outbound;

namespace PropostaService.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly PropostaDbContext _context;

    public UnitOfWork(PropostaDbContext context) => _context = context;

    public Task<int> SalvarAlteracoesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}

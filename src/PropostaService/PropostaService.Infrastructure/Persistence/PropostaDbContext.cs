using Microsoft.EntityFrameworkCore;
using PropostaService.Domain.Propostas;

namespace PropostaService.Infrastructure.Persistence;

public sealed class PropostaDbContext : DbContext
{
    /// <summary>Sequência do Postgres que alimenta o número amigável da proposta.</summary>
    public const string SequenciaNumeroProposta = "proposta_numero_seq";

    public PropostaDbContext(DbContextOptions<PropostaDbContext> options) : base(options)
    {
    }

    public DbSet<Proposta> Propostas => Set<Proposta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>(SequenciaNumeroProposta).StartsAt(1).IncrementsBy(1);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PropostaDbContext).Assembly);
    }
}

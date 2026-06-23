using MassTransit;
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

        // Inbox/Outbox do MassTransit. Aqui o serviço usa o Inbox para deduplicar
        // reentregas de ContratacaoRealizada (idempotência); o schema padrão (que
        // inclui as tabelas de Outbox) é mapeado por consistência com a config.
        modelBuilder.AddTransactionalOutboxEntities();
    }
}

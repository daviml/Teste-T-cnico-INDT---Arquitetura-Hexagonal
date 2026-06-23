using ContratacaoService.Domain.Contratacoes;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure.Persistence;

public sealed class ContratacaoDbContext : DbContext
{
    public ContratacaoDbContext(DbContextOptions<ContratacaoDbContext> options) : base(options)
    {
    }

    public DbSet<Contratacao> Contratacoes => Set<Contratacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContratacaoDbContext).Assembly);

        // Transactional Outbox (MassTransit): grava o integration event na mesma
        // transação do agregado; um serviço em background o entrega ao broker.
        // Schema padrão (Inbox/Outbox); aqui usamos apenas o Bus Outbox — a tabela
        // InboxState vem junto por ser referenciada por FK de OutboxMessage.
        modelBuilder.AddTransactionalOutboxEntities();
    }
}

using ContratacaoService.Domain.Contratacoes;
using ContratacaoService.Domain.Contratacoes.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContratacaoService.Infrastructure.Persistence.Configurations;

internal sealed class ContratacaoConfiguration : IEntityTypeConfiguration<Contratacao>
{
    public void Configure(EntityTypeBuilder<Contratacao> builder)
    {
        builder.ToTable("Contratacoes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.PropostaId).IsRequired();

        // Invariante crítica anti-duplicação: a verdade final é este índice único.
        builder.HasIndex(c => c.PropostaId).IsUnique();

        builder.Property(c => c.NumeroApolice)
            .HasConversion(apolice => apolice.Valor, valor => NumeroApolice.De(valor))
            .HasMaxLength(40)
            .IsRequired();
        builder.HasIndex(c => c.NumeroApolice).IsUnique();

        builder.Property(c => c.ValorPremioPago)
            .HasConversion(dinheiro => dinheiro.Valor, valor => Dinheiro.De(valor))
            .HasColumnType("numeric(18,2)");

        builder.Property(c => c.DataContratacao)
            .HasColumnType("timestamptz");

        builder.Ignore(c => c.EventosDeDominio);
    }
}

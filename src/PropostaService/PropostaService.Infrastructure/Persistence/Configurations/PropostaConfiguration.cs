using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropostaService.Domain.Propostas;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API da Proposta. Os Value Objects são persistidos via
/// conversores (texto/numérico) e reconstituídos pelas factories do domínio —
/// os dados no banco já são válidos, então a reconstituição não falha.
/// </summary>
internal sealed class PropostaConfiguration : IEntityTypeConfiguration<Proposta>
{
    public void Configure(EntityTypeBuilder<Proposta> builder)
    {
        builder.ToTable("Propostas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Numero)
            .HasConversion(numero => numero.Valor, valor => NumeroProposta.De(valor))
            .HasMaxLength(40)
            .IsRequired();
        builder.HasIndex(p => p.Numero).IsUnique();

        builder.Property(p => p.ClienteNome)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.ClienteDocumento)
            .HasConversion(documento => documento.Valor, valor => Documento.Criar(valor))
            .HasMaxLength(14)
            .IsRequired();
        builder.HasIndex(p => p.ClienteDocumento);

        builder.Property(p => p.ClienteEmail)
            .HasConversion(email => email.Valor, valor => Email.Criar(valor))
            .HasMaxLength(254)
            .IsRequired();

        // Sistema mono-moeda (BRL): persistimos só o valor; a moeda é
        // reconstituída como padrão. O schema não tem coluna de moeda.
        builder.Property(p => p.ValorCobertura)
            .HasConversion(dinheiro => dinheiro.Valor, valor => Dinheiro.De(valor))
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.ValorPremio)
            .HasConversion(dinheiro => dinheiro.Valor, valor => Dinheiro.De(valor))
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(p => p.Status);

        builder.Property(p => p.DataCriacao)
            .HasColumnType("timestamptz");

        // Concorrência otimista via coluna de sistema xmin do PostgreSQL.
        // (No Npgsql 10 o atalho UseXminAsConcurrencyToken foi removido; o
        // mapeamento da pseudo-coluna xmin como row version é o substituto.)
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // Eventos de domínio não são persistidos.
        builder.Ignore(p => p.EventosDeDominio);
    }
}

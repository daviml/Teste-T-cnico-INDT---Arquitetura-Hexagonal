using ContratacaoService.Domain.Contratacoes;
using ContratacaoService.Domain.Contratacoes.ValueObjects;
using ContratacaoService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Persistencia;

/// <summary>
/// Persistência do agregado Contratacao contra Postgres real: confirma o
/// mapeamento dos VOs (domínio puro, sem ctor de ORM) e a invariante crítica
/// anti-duplicação — o índice único de PropostaId é a fonte de verdade.
/// </summary>
public sealed class ContratacaoPersistenciaTests : PostgresTesteBase
{
    protected override async Task AoIniciarAsync()
    {
        await using var ctx = CriarContexto();
        await ctx.Database.MigrateAsync();
    }

    private ContratacaoDbContext CriarContexto()
        => new(new DbContextOptionsBuilder<ContratacaoDbContext>().UseNpgsql(ConnectionString).Options);

    [Fact]
    public async Task Persiste_e_recarrega_contratacao_com_value_objects_intactos()
    {
        var contratacao = Contratacao.Efetuar(Guid.NewGuid(), Dinheiro.De(1234.56m));

        await using (var ctx = CriarContexto())
        {
            ctx.Contratacoes.Add(contratacao);
            await ctx.SaveChangesAsync();
        }

        // Contexto novo: força ida ao banco (sem identity map).
        await using (var ctx = CriarContexto())
        {
            var recarregada = await ctx.Contratacoes.SingleAsync(c => c.Id == contratacao.Id);

            recarregada.PropostaId.Should().Be(contratacao.PropostaId);
            recarregada.NumeroApolice.Valor.Should().Be(contratacao.NumeroApolice.Valor);
            recarregada.ValorPremioPago.Valor.Should().Be(1234.56m);
            recarregada.DataContratacao.Should().BeCloseTo(contratacao.DataContratacao, TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public async Task Indice_unico_de_PropostaId_bloqueia_segunda_contratacao_da_mesma_proposta()
    {
        var propostaId = Guid.NewGuid();

        await using (var ctx = CriarContexto())
        {
            ctx.Contratacoes.Add(Contratacao.Efetuar(propostaId, Dinheiro.De(100m)));
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CriarContexto())
        {
            ctx.Contratacoes.Add(Contratacao.Efetuar(propostaId, Dinheiro.De(200m)));

            var salvar = async () => await ctx.SaveChangesAsync();

            await salvar.Should().ThrowAsync<DbUpdateException>();
        }
    }
}

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PropostaService.Domain.Propostas;
using PropostaService.Domain.Propostas.ValueObjects;
using PropostaService.Infrastructure.Persistence;

namespace IntegrationTests.Persistencia;

/// <summary>
/// Persistência do agregado Proposta contra Postgres real: round-trip dos VOs
/// (Documento/Email/Dinheiro/NumeroProposta) e máquina de estados, mais o índice
/// único de NumeroProposta.
/// </summary>
public sealed class PropostaPersistenciaTests : PostgresTesteBase
{
    protected override async Task AoIniciarAsync()
    {
        await using var ctx = CriarContexto();
        await ctx.Database.MigrateAsync();
    }

    private PropostaDbContext CriarContexto()
        => new(new DbContextOptionsBuilder<PropostaDbContext>().UseNpgsql(ConnectionString).Options);

    private static Proposta CriarProposta(NumeroProposta numero, decimal cobertura = 1000m)
        => Proposta.Criar(
            numero,
            "Cliente Teste",
            Documento.Criar("52998224725"),
            Email.Criar("cliente@teste.com"),
            Dinheiro.De(cobertura));

    [Fact]
    public async Task Persiste_e_recarrega_proposta_com_value_objects_e_status()
    {
        var proposta = CriarProposta(NumeroProposta.Gerar(2026, 1), cobertura: 5000m);

        await using (var ctx = CriarContexto())
        {
            ctx.Propostas.Add(proposta);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CriarContexto())
        {
            var recarregada = await ctx.Propostas.SingleAsync(p => p.Id == proposta.Id);

            recarregada.Numero.Valor.Should().Be(proposta.Numero.Valor);
            recarregada.ClienteNome.Should().Be("Cliente Teste");
            recarregada.ClienteDocumento.Valor.Should().Be("52998224725");
            recarregada.ClienteEmail.Valor.Should().Be("cliente@teste.com");
            recarregada.ValorCobertura.Valor.Should().Be(5000m);
            recarregada.ValorPremio.Valor.Should().Be(proposta.ValorPremio.Valor);
            recarregada.Status.Should().Be(StatusProposta.EmAnalise);
        }
    }

    [Fact]
    public async Task Indice_unico_de_Numero_bloqueia_propostas_com_mesmo_numero()
    {
        var numero = NumeroProposta.Gerar(2026, 42);

        await using (var ctx = CriarContexto())
        {
            ctx.Propostas.Add(CriarProposta(numero));
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CriarContexto())
        {
            ctx.Propostas.Add(CriarProposta(numero));

            var salvar = async () => await ctx.SaveChangesAsync();

            await salvar.Should().ThrowAsync<DbUpdateException>();
        }
    }
}

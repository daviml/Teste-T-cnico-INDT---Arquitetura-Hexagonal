using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Application.Common;
using PropostaService.Application.Ports.Inbound;
using PropostaService.Application.Propostas.UseCases;
using PropostaService.Domain.Propostas;
using PropostaService.Domain.Propostas.ValueObjects;
using PropostaService.Infrastructure;
using PropostaService.Infrastructure.Persistence;

namespace IntegrationTests.Mensageria;

/// <summary>
/// Idempotência ponta a ponta (Postgres real): processar o MESMO evento de
/// contratação duas vezes — como numa reentrega de mensagem — não duplica o
/// efeito. Exercita a fatia application + persistência (repositório, UnitOfWork,
/// token de concorrência xmin) que o consumer aciona, usando o wiring de produção
/// (<see cref="DependencyInjection.AddPropostaInfrastructure"/>).
/// </summary>
public sealed class ReentregaEventoTests : PostgresTesteBase
{
    private ServiceProvider _provider = null!;

    protected override async Task AoIniciarAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPropostaInfrastructure(ConnectionString);
        services.AddScoped<IMarcarComoContratada, MarcarComoContratadaHandler>();
        _provider = services.BuildServiceProvider(validateScopes: true);

        await using var scope = _provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<PropostaDbContext>().Database.MigrateAsync();
    }

    protected override async Task AoDescartarAsync() => await _provider.DisposeAsync();

    [Fact]
    public async Task Processar_o_mesmo_evento_duas_vezes_nao_duplica_o_efeito()
    {
        // Arrange: proposta Aprovada persistida (pré-condição para contratar).
        var proposta = Proposta.Criar(
            NumeroProposta.Gerar(2026, 7),
            "Cliente Teste",
            Documento.Criar("52998224725"),
            Email.Criar("cliente@teste.com"),
            Dinheiro.De(1000m));
        proposta.Aprovar();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
            ctx.Propostas.Add(proposta);
            await ctx.SaveChangesAsync();
        }

        // Act: duas entregas do mesmo evento (escopos distintos, como 2 mensagens).
        var primeira = await MarcarComoContratada(proposta.Id);
        var segunda = await MarcarComoContratada(proposta.Id);

        // Assert: ambas bem-sucedidas e a proposta fica Contratada UMA vez (no-op na 2ª).
        primeira.Sucesso.Should().BeTrue();
        segunda.Sucesso.Should().BeTrue();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
            var recarregada = await ctx.Propostas.SingleAsync(p => p.Id == proposta.Id);
            recarregada.Status.Should().Be(StatusProposta.Contratada);
        }
    }

    private async Task<Result> MarcarComoContratada(Guid propostaId)
    {
        await using var scope = _provider.CreateAsyncScope();
        var caso = scope.ServiceProvider.GetRequiredService<IMarcarComoContratada>();
        return await caso.ExecutarAsync(propostaId);
    }
}

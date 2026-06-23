using ContratacaoService.Application.Common;
using ContratacaoService.Application.Contratacoes.Dtos;
using ContratacaoService.Application.Ports.Inbound;
using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Application.Contratacoes.UseCases;
using ContratacaoService.Infrastructure;
using ContratacaoService.Infrastructure.Messaging;
using ContratacaoService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Mensageria;

/// <summary>
/// Resiliência do Transactional Outbox (Postgres real): contratar com o RabbitMQ
/// INDISPONÍVEL ainda assim persiste a contratação E grava o integration event na
/// tabela de Outbox, na MESMA transação. Usa o wiring de produção
/// (<see cref="DependencyInjection.AddContratacaoInfrastructure"/>) apontando o broker
/// para um host inexistente e SEM iniciar o bus (os hosted services não sobem num
/// <c>BuildServiceProvider</c>) — exatamente o estado "broker fora do ar, mensagem
/// retida para entrega posterior". A entrega em si ao broker é verificada em runtime
/// (não automatizada, para evitar dependência de broker no harness de testes).
/// </summary>
public sealed class OutboxResilienciaTests : PostgresTesteBase
{
    private ServiceProvider _provider = null!;

    protected override async Task AoIniciarAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Wiring real: EF Outbox + Bus Outbox sobre o ContratacaoDbContext. O broker
        // aponta para um host inexistente de propósito — nunca é contatado porque o
        // bus não é iniciado neste provider.
        services.AddContratacaoInfrastructure(
            ConnectionString,
            new Uri("http://proposta-service.invalido"),
            new RabbitMqOptions("rabbitmq.invalido", "guest", "guest"));

        // Substitui o gateway HTTP real por um fake que devolve a proposta Aprovada,
        // isolando o teste do PropostaService (o foco aqui é o Outbox, não o gateway).
        services.AddScoped<IPropostaGateway, PropostaGatewayFake>();
        services.AddScoped<IContratarProposta, ContratarPropostaHandler>();

        _provider = services.BuildServiceProvider(validateScopes: true);

        await using var scope = _provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>().Database.MigrateAsync();
    }

    protected override async Task AoDescartarAsync() => await _provider.DisposeAsync();

    [Fact]
    public async Task Contratar_com_broker_indisponivel_persiste_contratacao_e_retem_evento_no_outbox()
    {
        var propostaId = Guid.NewGuid();

        // Act: contratar enquanto o broker está inacessível.
        Result<ContratacaoResponse> resultado;
        await using (var scope = _provider.CreateAsyncScope())
        {
            var contratar = scope.ServiceProvider.GetRequiredService<IContratarProposta>();
            resultado = await contratar.ExecutarAsync(new ContratarPropostaRequest(propostaId));
        }

        // Assert: a API teria respondido sucesso mesmo com o broker fora do ar.
        resultado.Sucesso.Should().BeTrue();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();

            // A contratação foi persistida...
            var contratacoes = await ctx.Contratacoes.CountAsync(c => c.PropostaId == propostaId);
            contratacoes.Should().Be(1);

            // ...e o evento ficou DURÁVEL na tabela de Outbox do MassTransit,
            // aguardando entrega (a transação confirmou as duas escritas juntas).
            var mensagensNoOutbox = await ctx.Database
                .SqlQueryRaw<int>("""SELECT COUNT(*)::int AS "Value" FROM "OutboxMessage" """)
                .SingleAsync();
            mensagensNoOutbox.Should().BeGreaterThan(0);
        }
    }

    /// <summary>Gateway de teste: a proposta consultada está sempre Aprovada.</summary>
    private sealed class PropostaGatewayFake : IPropostaGateway
    {
        public Task<Result<PropostaConsultada>> ObterPropostaAsync(Guid propostaId, CancellationToken ct = default)
            => Task.FromResult(Result.Ok(new PropostaConsultada(propostaId, EstaAprovada: true, ValorPremio: 1500m)));
    }
}

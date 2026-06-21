using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Contratacoes;
using ContratacaoService.Domain.Contratacoes.Events;
using ContratacaoService.Domain.Contratacoes.ValueObjects;
using FluentAssertions;

namespace ContratacaoService.Tests.Domain;

public class ContratacaoTests
{
    [Fact]
    public void Efetuar_DevePreencherCampos_GerarApoliceERegistrarEvento()
    {
        var propostaId = Guid.NewGuid();

        var contratacao = Contratacao.Efetuar(propostaId, Dinheiro.De(500m));

        contratacao.Id.Should().NotBe(Guid.Empty);
        contratacao.PropostaId.Should().Be(propostaId);
        contratacao.ValorPremioPago.Valor.Should().Be(500m);
        contratacao.NumeroApolice.Valor.Should().StartWith("APO-");
        contratacao.DataContratacao.Should().NotBe(default);

        contratacao.EventosDeDominio.Should().ContainSingle(e => e is ContratacaoEfetuadaEvent);
    }

    [Fact]
    public void Efetuar_DeveEmitirEventoComDadosDaContratacao()
    {
        var propostaId = Guid.NewGuid();

        var contratacao = Contratacao.Efetuar(propostaId, Dinheiro.De(500m));

        var evento = contratacao.EventosDeDominio.OfType<ContratacaoEfetuadaEvent>().Single();
        evento.ContratacaoId.Should().Be(contratacao.Id);
        evento.PropostaId.Should().Be(propostaId);
        evento.NumeroApolice.Should().Be(contratacao.NumeroApolice.Valor);
        evento.ValorPremioPago.Should().Be(500m);
    }

    [Fact]
    public void Efetuar_ComPropostaIdVazio_DeveLancarDomainException()
    {
        var acao = () => Contratacao.Efetuar(Guid.Empty, Dinheiro.De(500m));

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Efetuar_ComPremioZero_DeveLancarDomainException()
    {
        var acao = () => Contratacao.Efetuar(Guid.NewGuid(), Dinheiro.De(0m));

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Efetuar_DuasVezes_DeveGerarApolicesEIdsDistintos()
    {
        var propostaId = Guid.NewGuid();

        var a = Contratacao.Efetuar(propostaId, Dinheiro.De(500m));
        var b = Contratacao.Efetuar(propostaId, Dinheiro.De(500m));

        a.Id.Should().NotBe(b.Id);
        a.NumeroApolice.Should().NotBe(b.NumeroApolice);
    }
}

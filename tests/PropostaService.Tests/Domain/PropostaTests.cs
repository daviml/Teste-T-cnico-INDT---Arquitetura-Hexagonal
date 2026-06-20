using FluentAssertions;
using PropostaService.Domain.Common;
using PropostaService.Domain.Propostas;
using PropostaService.Domain.Propostas.Events;
using PropostaService.Domain.Propostas.Exceptions;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Tests.Domain;

public class PropostaTests
{
    [Fact]
    public void Criar_DeveIniciarEmAnalise_DerivarPremioERegistrarEvento()
    {
        var proposta = Proposta.Criar(
            NumeroProposta.Gerar(2026, 1),
            "  Cliente Teste  ",
            Documento.Criar("52998224725"),
            Email.Criar("cliente@teste.com"),
            Dinheiro.De(1000m));

        proposta.Id.Should().NotBe(Guid.Empty);
        proposta.Status.Should().Be(StatusProposta.EmAnalise);
        proposta.ClienteNome.Should().Be("Cliente Teste"); // trim aplicado
        proposta.ValorPremio.Valor.Should().Be(50m);        // 5% de 1000
        proposta.EventosDeDominio.Should().ContainSingle(e => e is PropostaCriadaEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemNomeDoCliente_DeveLancarDomainException(string nome)
    {
        var acao = () => Proposta.Criar(
            NumeroProposta.Gerar(2026, 1),
            nome,
            Documento.Criar("52998224725"),
            Email.Criar("cliente@teste.com"),
            Dinheiro.De(1000m));

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_ComCoberturaZeroOuNegativa_DeveLancarDomainException()
    {
        var acao = () => Proposta.Criar(
            NumeroProposta.Gerar(2026, 1),
            "Cliente Teste",
            Documento.Criar("52998224725"),
            Email.Criar("cliente@teste.com"),
            Dinheiro.De(0m));

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Aprovar_APartirDeEmAnalise_DeveTransicionarERegistrarEvento()
    {
        var proposta = PropostaBuilder.Valida();

        proposta.Aprovar();

        proposta.Status.Should().Be(StatusProposta.Aprovada);
        proposta.EventosDeDominio.Should().ContainSingle(e => e is PropostaAprovadaEvent);
    }

    [Fact]
    public void Rejeitar_APartirDeEmAnalise_DeveTransicionarERegistrarEvento()
    {
        var proposta = PropostaBuilder.Valida();

        proposta.Rejeitar();

        proposta.Status.Should().Be(StatusProposta.Rejeitada);
        proposta.EventosDeDominio.Should().ContainSingle(e => e is PropostaRejeitadaEvent);
    }

    [Fact]
    public void Aprovar_UmaPropostaJaRejeitada_DeveLancarTransicaoInvalida()
    {
        var proposta = PropostaBuilder.Valida();
        proposta.Rejeitar();

        var acao = proposta.Aprovar;

        acao.Should().Throw<TransicaoDeStatusInvalidaException>();
    }

    [Fact]
    public void Rejeitar_UmaPropostaJaAprovada_DeveLancarTransicaoInvalida()
    {
        var proposta = PropostaBuilder.Valida();
        proposta.Aprovar();

        var acao = proposta.Rejeitar;

        acao.Should().Throw<TransicaoDeStatusInvalidaException>();
    }

    [Fact]
    public void MarcarComoContratada_APartirDeAprovada_DeveTransicionarERegistrarEvento()
    {
        var proposta = PropostaBuilder.Valida();
        proposta.Aprovar();
        proposta.LimparEventos();

        proposta.MarcarComoContratada();

        proposta.Status.Should().Be(StatusProposta.Contratada);
        proposta.EventosDeDominio.Should().ContainSingle(e => e is PropostaContratadaEvent);
    }

    [Fact]
    public void MarcarComoContratada_SemEstarAprovada_DeveLancarTransicaoInvalida()
    {
        var proposta = PropostaBuilder.Valida(); // ainda EmAnalise

        var acao = proposta.MarcarComoContratada;

        acao.Should().Throw<TransicaoDeStatusInvalidaException>();
    }

    [Fact]
    public void MarcarComoContratada_QuandoJaContratada_DeveSerIdempotente()
    {
        var proposta = PropostaBuilder.Valida();
        proposta.Aprovar();
        proposta.MarcarComoContratada();
        proposta.LimparEventos();

        var acao = proposta.MarcarComoContratada;

        acao.Should().NotThrow();
        proposta.Status.Should().Be(StatusProposta.Contratada);
        proposta.EventosDeDominio.Should().BeEmpty(); // no-op: nenhum novo evento
    }
}

using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Contratacoes.ValueObjects;
using FluentAssertions;

namespace ContratacaoService.Tests.Domain;

public class DinheiroTests
{
    [Fact]
    public void De_ComValorPositivo_DeveUsarBrlComoPadrao()
    {
        var dinheiro = Dinheiro.De(100.50m);

        dinheiro.Valor.Should().Be(100.50m);
        dinheiro.Moeda.Should().Be("BRL");
    }

    [Fact]
    public void De_ComValorNegativo_DeveLancarDomainException()
    {
        var acao = () => Dinheiro.De(-1m);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void De_DeveArredondarParaDuasCasas()
    {
        Dinheiro.De(10.015m).Valor.Should().Be(10.02m);
    }

    [Fact]
    public void Dinheiros_ComMesmoValorEMoeda_DevemSerIguaisPorValor()
    {
        Dinheiro.De(10m).Should().Be(Dinheiro.De(10m));
    }
}

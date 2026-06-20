using FluentAssertions;
using PropostaService.Domain.Common;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Tests.Domain.ValueObjects;

public class DinheiroTests
{
    [Fact]
    public void De_ComValorPositivo_DeveUsarBrlComoMoedaPadrao()
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
        Dinheiro.De(10.005m).Valor.Should().Be(10.00m); // arredondamento bancário
        Dinheiro.De(10.015m).Valor.Should().Be(10.02m);
    }

    [Fact]
    public void Multiplicar_DeveAplicarFatorMantendoMoeda()
    {
        var resultado = Dinheiro.De(200m).Multiplicar(0.05m);

        resultado.Valor.Should().Be(10m);
        resultado.Moeda.Should().Be("BRL");
    }

    [Fact]
    public void Somar_ComMoedasDistintas_DeveLancarDomainException()
    {
        var acao = () => Dinheiro.De(10m, "BRL").Somar(Dinheiro.De(10m, "USD"));

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Somar_ComMesmaMoeda_DeveSomarValores()
    {
        Dinheiro.De(10m).Somar(Dinheiro.De(5m)).Valor.Should().Be(15m);
    }

    [Fact]
    public void Dinheiros_ComMesmoValorEMoeda_DevemSerIguaisPorValor()
    {
        Dinheiro.De(10m).Should().Be(Dinheiro.De(10m));
    }
}

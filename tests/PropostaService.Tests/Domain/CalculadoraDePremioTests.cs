using FluentAssertions;
using PropostaService.Domain.Propostas;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Tests.Domain;

public class CalculadoraDePremioTests
{
    [Theory]
    [InlineData(1000, 50)]
    [InlineData(200, 10)]
    [InlineData(0, 0)]
    public void Calcular_DeveAplicarTaxaBaseSobreCobertura(decimal cobertura, decimal premioEsperado)
    {
        var premio = CalculadoraDePremio.Calcular(Dinheiro.De(cobertura));

        premio.Valor.Should().Be(premioEsperado);
        premio.Moeda.Should().Be("BRL");
    }

    [Fact]
    public void TaxaBase_DeveSerCincoPorCento()
    {
        CalculadoraDePremio.TaxaBase.Should().Be(0.05m);
    }
}

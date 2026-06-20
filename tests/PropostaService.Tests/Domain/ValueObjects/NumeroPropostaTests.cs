using FluentAssertions;
using PropostaService.Domain.Common;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Tests.Domain.ValueObjects;

public class NumeroPropostaTests
{
    [Fact]
    public void Gerar_DeveFormatarComoPrpAnoSequencialComSeisDigitos()
    {
        var numero = NumeroProposta.Gerar(2026, 42);

        numero.Valor.Should().Be("PRP-2026-000042");
    }

    [Theory]
    [InlineData(1999, 1)]   // ano inválido
    [InlineData(2026, 0)]   // sequencial não positivo
    [InlineData(2026, -5)]
    public void Gerar_ComParametrosInvalidos_DeveLancarDomainException(int ano, long sequencial)
    {
        var acao = () => NumeroProposta.Gerar(ano, sequencial);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void De_ComFormatoValido_DeveReconstituir()
    {
        NumeroProposta.De("PRP-2026-000042").Valor.Should().Be("PRP-2026-000042");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("2026-000042")]
    [InlineData("PRP-26-000042")]
    [InlineData("PRP-2026-42")] // menos de 6 dígitos
    public void De_ComFormatoInvalido_DeveLancarDomainException(string? entrada)
    {
        var acao = () => NumeroProposta.De(entrada);

        acao.Should().Throw<DomainException>();
    }
}

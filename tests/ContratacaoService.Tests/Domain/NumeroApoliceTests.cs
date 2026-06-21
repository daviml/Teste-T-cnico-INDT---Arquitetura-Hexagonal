using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Contratacoes.ValueObjects;
using FluentAssertions;

namespace ContratacaoService.Tests.Domain;

public class NumeroApoliceTests
{
    [Fact]
    public void Gerar_DeveSeguirFormatoApoAnoSufixoDeDozeHex()
    {
        var referencia = Guid.Parse("a1b2c3d4e5f60718293a4b5c6d7e8f90");

        var numero = NumeroApolice.Gerar(2026, referencia);

        numero.Valor.Should().Be("APO-2026-A1B2C3D4E5F6");
    }

    [Fact]
    public void Gerar_ComAnoInvalido_DeveLancarDomainException()
    {
        var acao = () => NumeroApolice.Gerar(1999, Guid.NewGuid());

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Gerar_ComReferenciasDistintas_DeveProduzirNumerosDistintos()
    {
        var a = NumeroApolice.Gerar(2026, Guid.NewGuid());
        var b = NumeroApolice.Gerar(2026, Guid.NewGuid());

        a.Should().NotBe(b);
    }

    [Fact]
    public void De_ComFormatoValido_DeveReconstituir()
    {
        NumeroApolice.De("APO-2026-A1B2C3D4E5F6").Valor.Should().Be("APO-2026-A1B2C3D4E5F6");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("APO-2026-XYZ")]            // sufixo curto / não-hex
    [InlineData("APO-26-A1B2C3D4E5F6")]     // ano com 2 dígitos
    [InlineData("APX-2026-A1B2C3D4E5F6")]   // prefixo errado
    public void De_ComFormatoInvalido_DeveLancarDomainException(string? entrada)
    {
        var acao = () => NumeroApolice.De(entrada);

        acao.Should().Throw<DomainException>();
    }
}

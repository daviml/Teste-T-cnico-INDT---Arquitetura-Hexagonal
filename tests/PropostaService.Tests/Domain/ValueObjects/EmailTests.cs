using FluentAssertions;
using PropostaService.Domain.Common;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("  Joao.Silva@Empresa.COM ", "joao.silva@empresa.com")]
    [InlineData("user@dominio.com.br", "user@dominio.com.br")]
    public void Criar_ComEmailValido_DeveNormalizarParaMinusculoSemEspacos(string entrada, string esperado)
    {
        var email = Email.Criar(entrada);

        email.Valor.Should().Be(esperado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sem-arroba")]
    [InlineData("sem@dominio")]   // sem ponto no domínio
    [InlineData("@dominio.com")]  // sem parte local
    [InlineData("a b@c.com")]     // espaço no meio
    public void Criar_ComEmailInvalido_DeveLancarDomainException(string? entrada)
    {
        var acao = () => Email.Criar(entrada);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Emails_EquivalentesAposNormalizacao_DevemSerIguais()
    {
        Email.Criar("TESTE@X.COM").Should().Be(Email.Criar("teste@x.com"));
    }
}

using FluentAssertions;
using PropostaService.Domain.Common;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Tests.Domain.ValueObjects;

public class DocumentoTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")] // aceita com máscara
    public void Criar_ComCpfValido_DeveNormalizarEClassificarComoCpf(string entrada)
    {
        var documento = Documento.Criar(entrada);

        documento.Valor.Should().Be("52998224725");
        documento.Tipo.Should().Be(TipoDocumento.Cpf);
    }

    [Theory]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")] // aceita com máscara
    public void Criar_ComCnpjValido_DeveNormalizarEClassificarComoCnpj(string entrada)
    {
        var documento = Documento.Criar(entrada);

        documento.Valor.Should().Be("11222333000181");
        documento.Tipo.Should().Be(TipoDocumento.Cnpj);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]                 // tamanho inválido
    [InlineData("52998224724")]         // CPF com dígito verificador errado
    [InlineData("11111111111")]         // CPF com todos os dígitos iguais
    [InlineData("11222333000180")]      // CNPJ com dígito verificador errado
    [InlineData("00000000000000")]      // CNPJ com todos os dígitos iguais
    public void Criar_ComDocumentoInvalido_DeveLancarDomainException(string? entrada)
    {
        var acao = () => Documento.Criar(entrada);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Documentos_ComMesmoValor_DevemSerIguaisPorValor()
    {
        var a = Documento.Criar("529.982.247-25");
        var b = Documento.Criar("52998224725");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }
}

using FluentAssertions;
using NSubstitute;
using PropostaService.Application.Common;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Application.Propostas.UseCases;
using PropostaService.Domain.Propostas;
using PropostaService.Tests.Domain;

namespace PropostaService.Tests.Application;

public class ObterPropostaHandlerTests
{
    private readonly IPropostaRepository _repositorio = Substitute.For<IPropostaRepository>();
    private readonly ObterPropostaHandler _handler;

    public ObterPropostaHandlerTests() => _handler = new ObterPropostaHandler(_repositorio);

    [Fact]
    public async Task ExecutarAsync_QuandoExiste_DeveRetornarResposta()
    {
        var proposta = PropostaBuilder.Valida();
        _repositorio.ObterPorIdAsync(proposta.Id, Arg.Any<CancellationToken>()).Returns(proposta);

        var resultado = await _handler.ExecutarAsync(proposta.Id);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Id.Should().Be(proposta.Id);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoNaoExiste_DeveRetornarNaoEncontrado()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Proposta?)null);

        var resultado = await _handler.ExecutarAsync(Guid.NewGuid());

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.NaoEncontrado);
    }
}

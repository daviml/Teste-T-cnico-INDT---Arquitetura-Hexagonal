using ContratacaoService.Application.Common;
using ContratacaoService.Application.Contratacoes.UseCases;
using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.Contratacoes;
using ContratacaoService.Domain.Contratacoes.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace ContratacaoService.Tests.Application;

public class ObterContratacaoHandlerTests
{
    private readonly IContratacaoRepository _repositorio = Substitute.For<IContratacaoRepository>();
    private readonly ObterContratacaoHandler _handler;

    public ObterContratacaoHandlerTests() => _handler = new ObterContratacaoHandler(_repositorio);

    [Fact]
    public async Task ExecutarAsync_QuandoExiste_DeveRetornarResposta()
    {
        var contratacao = Contratacao.Efetuar(Guid.NewGuid(), Dinheiro.De(500m));
        _repositorio.ObterPorIdAsync(contratacao.Id, Arg.Any<CancellationToken>()).Returns(contratacao);

        var resultado = await _handler.ExecutarAsync(contratacao.Id);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Id.Should().Be(contratacao.Id);
        resultado.Valor.NumeroApolice.Should().Be(contratacao.NumeroApolice.Valor);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoNaoExiste_DeveRetornarNaoEncontrado()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Contratacao?)null);

        var resultado = await _handler.ExecutarAsync(Guid.NewGuid());

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.NaoEncontrado);
    }
}

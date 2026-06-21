using FluentAssertions;
using NSubstitute;
using PropostaService.Application.Common;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Application.Propostas.UseCases;
using PropostaService.Domain.Propostas;
using PropostaService.Tests.Domain;

namespace PropostaService.Tests.Application;

public class MarcarComoContratadaHandlerTests
{
    private readonly IPropostaRepository _repositorio = Substitute.For<IPropostaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly MarcarComoContratadaHandler _handler;

    public MarcarComoContratadaHandlerTests() =>
        _handler = new MarcarComoContratadaHandler(_repositorio, _unitOfWork);

    [Fact]
    public async Task ExecutarAsync_QuandoAprovada_DeveContratarESalvar()
    {
        var proposta = PropostaBuilder.Valida();
        proposta.Aprovar();
        _repositorio.ObterPorIdAsync(proposta.Id, Arg.Any<CancellationToken>()).Returns(proposta);

        var resultado = await _handler.ExecutarAsync(proposta.Id);

        resultado.Sucesso.Should().BeTrue();
        proposta.Status.Should().Be(StatusProposta.Contratada);
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_QuandoNaoAprovada_DeveRetornarConflito()
    {
        var proposta = PropostaBuilder.Valida(); // ainda EmAnalise
        _repositorio.ObterPorIdAsync(proposta.Id, Arg.Any<CancellationToken>()).Returns(proposta);

        var resultado = await _handler.ExecutarAsync(proposta.Id);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.Conflito);
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

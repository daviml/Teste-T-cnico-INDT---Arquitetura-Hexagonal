using FluentAssertions;
using NSubstitute;
using PropostaService.Application.Common;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Application.Propostas.Dtos;
using PropostaService.Application.Propostas.UseCases;
using PropostaService.Domain.Propostas;
using PropostaService.Tests.Domain;

namespace PropostaService.Tests.Application;

public class AlterarStatusPropostaHandlerTests
{
    private readonly IPropostaRepository _repositorio = Substitute.For<IPropostaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AlterarStatusPropostaHandler _handler;

    public AlterarStatusPropostaHandlerTests() =>
        _handler = new AlterarStatusPropostaHandler(_repositorio, _unitOfWork);

    [Fact]
    public async Task ExecutarAsync_AprovarEmAnalise_DeveTransicionarESalvar()
    {
        var proposta = PropostaBuilder.Valida();
        _repositorio.ObterPorIdAsync(proposta.Id, Arg.Any<CancellationToken>()).Returns(proposta);

        var resultado = await _handler.ExecutarAsync(proposta.Id, new AlterarStatusRequest(StatusPropostaDestino.Aprovada));

        resultado.Sucesso.Should().BeTrue();
        proposta.Status.Should().Be(StatusProposta.Aprovada);
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_QuandoNaoExiste_DeveRetornarNaoEncontrado()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Proposta?)null);

        var resultado = await _handler.ExecutarAsync(Guid.NewGuid(), new AlterarStatusRequest(StatusPropostaDestino.Aprovada));

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.NaoEncontrado);
        await _unitOfWork.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_TransicaoInvalida_DeveRetornarConflito()
    {
        var proposta = PropostaBuilder.Valida();
        proposta.Rejeitar(); // já rejeitada
        _repositorio.ObterPorIdAsync(proposta.Id, Arg.Any<CancellationToken>()).Returns(proposta);

        var resultado = await _handler.ExecutarAsync(proposta.Id, new AlterarStatusRequest(StatusPropostaDestino.Aprovada));

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.Conflito);
        await _unitOfWork.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }
}

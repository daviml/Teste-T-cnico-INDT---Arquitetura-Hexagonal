using ContratacaoService.Application.Common;
using ContratacaoService.Application.Contratacoes.Dtos;
using ContratacaoService.Application.Contratacoes.UseCases;
using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.Contratacoes;
using ContratacaoService.Domain.Contratacoes.Events;
using FluentAssertions;
using NSubstitute;

namespace ContratacaoService.Tests.Application;

public class ContratarPropostaHandlerTests
{
    private readonly IContratacaoRepository _repositorio = Substitute.For<IContratacaoRepository>();
    private readonly IPropostaGateway _gateway = Substitute.For<IPropostaGateway>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ContratarPropostaHandler _handler;

    private readonly Guid _propostaId = Guid.NewGuid();

    public ContratarPropostaHandlerTests()
        => _handler = new ContratarPropostaHandler(_repositorio, _gateway, _eventBus, _unitOfWork);

    private void GatewayRetorna(bool aprovada, decimal premio = 500m)
        => _gateway.ObterPropostaAsync(_propostaId, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new PropostaConsultada(_propostaId, aprovada, premio)));

    [Fact]
    public async Task ExecutarAsync_PropostaAprovada_DeveContratarPublicarEventoESalvar()
    {
        GatewayRetorna(aprovada: true, premio: 500m);

        var resultado = await _handler.ExecutarAsync(new ContratarPropostaRequest(_propostaId));

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.PropostaId.Should().Be(_propostaId);
        resultado.Valor.ValorPremioPago.Should().Be(500m);
        resultado.Valor.NumeroApolice.Should().StartWith("APO-");

        await _repositorio.Received(1).AdicionarAsync(Arg.Any<Contratacao>(), Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublicarAsync(Arg.Any<ContratacaoEfetuadaEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_QuandoJaContratada_DeveRetornarConflitoSemConsultarGateway()
    {
        _repositorio.ExisteParaPropostaAsync(_propostaId, Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await _handler.ExecutarAsync(new ContratarPropostaRequest(_propostaId));

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.Conflito);
        await _gateway.DidNotReceive().ObterPropostaAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Contratacao>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_QuandoPropostaNaoAprovada_DeveRetornarConflito()
    {
        GatewayRetorna(aprovada: false);

        var resultado = await _handler.ExecutarAsync(new ContratarPropostaRequest(_propostaId));

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.Conflito);
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Contratacao>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_QuandoGatewayIndisponivel_DevePropagarIndisponivelSemPersistir()
    {
        _gateway.ObterPropostaAsync(_propostaId, Arg.Any<CancellationToken>())
            .Returns(Result.Falhar<PropostaConsultada>(Erro.Indisponivel("PropostaService fora do ar.")));

        var resultado = await _handler.ExecutarAsync(new ContratarPropostaRequest(_propostaId));

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.Indisponivel);
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Contratacao>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_QuandoPropostaNaoEncontrada_DevePropagarNaoEncontrado()
    {
        _gateway.ObterPropostaAsync(_propostaId, Arg.Any<CancellationToken>())
            .Returns(Result.Falhar<PropostaConsultada>(Erro.NaoEncontrado("Proposta não encontrada.")));

        var resultado = await _handler.ExecutarAsync(new ContratarPropostaRequest(_propostaId));

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.NaoEncontrado);
    }
}

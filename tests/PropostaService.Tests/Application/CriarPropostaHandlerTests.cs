using FluentAssertions;
using NSubstitute;
using PropostaService.Application.Common;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Application.Propostas.Dtos;
using PropostaService.Application.Propostas.UseCases;
using PropostaService.Domain.Propostas;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Tests.Application;

public class CriarPropostaHandlerTests
{
    private readonly IPropostaRepository _repositorio = Substitute.For<IPropostaRepository>();
    private readonly IGeradorNumeroProposta _gerador = Substitute.For<IGeradorNumeroProposta>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CriarPropostaHandler _handler;

    public CriarPropostaHandlerTests()
    {
        _gerador.GerarProximoAsync(Arg.Any<CancellationToken>())
            .Returns(NumeroProposta.Gerar(2026, 1));
        _handler = new CriarPropostaHandler(_repositorio, _gerador, _unitOfWork);
    }

    private static CriarPropostaRequest RequestValido(decimal cobertura = 1000m) =>
        new("Cliente Teste", "52998224725", "cliente@teste.com", cobertura);

    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_DevePersistirERetornarResposta()
    {
        var resultado = await _handler.ExecutarAsync(RequestValido());

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.NumeroProposta.Should().Be("PRP-2026-000001");
        resultado.Valor.Status.Should().Be(nameof(StatusProposta.EmAnalise));
        resultado.Valor.ValorPremio.Should().Be(50m); // 5% de 1000

        await _repositorio.Received(1).AdicionarAsync(Arg.Any<Proposta>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_ComDocumentoInvalido_DeveFalharSemPersistir()
    {
        var request = RequestValido() with { ClienteDocumento = "111" };

        var resultado = await _handler.ExecutarAsync(request);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.Validacao);

        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Proposta>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_ComCoberturaZero_DeveFalharComValidacao()
    {
        var resultado = await _handler.ExecutarAsync(RequestValido(cobertura: 0m));

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.Validacao);
    }
}

using FluentAssertions;
using NSubstitute;
using PropostaService.Application.Common;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Application.Propostas.Dtos;
using PropostaService.Application.Propostas.UseCases;
using PropostaService.Domain.Propostas;
using PropostaService.Tests.Domain;

namespace PropostaService.Tests.Application;

public class ListarPropostasHandlerTests
{
    private readonly IPropostaRepository _repositorio = Substitute.For<IPropostaRepository>();
    private readonly ListarPropostasHandler _handler;

    public ListarPropostasHandlerTests() => _handler = new ListarPropostasHandler(_repositorio);

    [Fact]
    public async Task ExecutarAsync_DeveMapearItensETotal()
    {
        IReadOnlyList<Proposta> itens = [PropostaBuilder.Valida(), PropostaBuilder.Valida()];
        _repositorio.ListarAsync(null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((itens, 2L));

        var resultado = await _handler.ExecutarAsync(new ListarPropostasFiltro());

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Itens.Should().HaveCount(2);
        resultado.Valor.Total.Should().Be(2);
    }

    [Fact]
    public async Task ExecutarAsync_ComStatusInvalido_DeveRetornarValidacao()
    {
        var resultado = await _handler.ExecutarAsync(new ListarPropostasFiltro { Status = "Inexistente" });

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.Validacao);
    }

    [Fact]
    public async Task ExecutarAsync_DeveClampearTamanhoDePaginaEParsearStatus()
    {
        IReadOnlyList<Proposta> vazio = [];
        _repositorio.ListarAsync(StatusProposta.Aprovada, null, 1, 100, Arg.Any<CancellationToken>())
            .Returns((vazio, 0L));

        var resultado = await _handler.ExecutarAsync(new ListarPropostasFiltro
        {
            Status = "aprovada", // case-insensitive
            Pagina = 0,          // será corrigido para 1
            TamanhoPagina = 5000 // será limitado a 100
        });

        resultado.Sucesso.Should().BeTrue();
        await _repositorio.Received(1).ListarAsync(StatusProposta.Aprovada, null, 1, 100, Arg.Any<CancellationToken>());
    }
}

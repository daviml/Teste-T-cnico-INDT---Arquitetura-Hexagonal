using PropostaService.Application.Common;
using PropostaService.Application.Ports.Inbound;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Application.Propostas.Dtos;
using PropostaService.Domain.Propostas;

namespace PropostaService.Application.Propostas.UseCases;

public sealed class ListarPropostasHandler : IListarPropostas
{
    private const int TamanhoPaginaMaximo = 100;

    private readonly IPropostaRepository _repositorio;

    public ListarPropostasHandler(IPropostaRepository repositorio) => _repositorio = repositorio;

    public async Task<Result<ResultadoPaginado<PropostaResponse>>> ExecutarAsync(
        ListarPropostasFiltro filtro,
        CancellationToken ct = default)
    {
        StatusProposta? status = null;
        if (!string.IsNullOrWhiteSpace(filtro.Status))
        {
            if (!Enum.TryParse(filtro.Status, ignoreCase: true, out StatusProposta statusConvertido))
            {
                return Result.Falhar<ResultadoPaginado<PropostaResponse>>(
                    Erro.Validacao($"Status inválido: '{filtro.Status}'."));
            }

            status = statusConvertido;
        }

        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamanhoPagina = Math.Clamp(filtro.TamanhoPagina, 1, TamanhoPaginaMaximo);

        var (itens, total) = await _repositorio.ListarAsync(status, filtro.Documento, pagina, tamanhoPagina, ct);

        var resposta = new ResultadoPaginado<PropostaResponse>(
            [.. itens.Select(PropostaMapper.ParaResponse)],
            pagina,
            tamanhoPagina,
            total);

        return Result.Ok(resposta);
    }
}

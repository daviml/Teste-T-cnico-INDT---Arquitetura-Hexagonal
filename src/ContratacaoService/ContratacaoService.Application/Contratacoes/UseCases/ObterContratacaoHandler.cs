using ContratacaoService.Application.Common;
using ContratacaoService.Application.Contratacoes.Dtos;
using ContratacaoService.Application.Ports.Inbound;
using ContratacaoService.Application.Ports.Outbound;

namespace ContratacaoService.Application.Contratacoes.UseCases;

public sealed class ObterContratacaoHandler : IObterContratacao
{
    private readonly IContratacaoRepository _repositorio;

    public ObterContratacaoHandler(IContratacaoRepository repositorio) => _repositorio = repositorio;

    public async Task<Result<ContratacaoResponse>> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var contratacao = await _repositorio.ObterPorIdAsync(id, ct);

        return contratacao is null
            ? Result.Falhar<ContratacaoResponse>(Erro.NaoEncontrado($"Contratação {id} não encontrada."))
            : Result.Ok(ContratacaoMapper.ParaResponse(contratacao));
    }
}

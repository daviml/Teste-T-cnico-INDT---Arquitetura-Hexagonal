using PropostaService.Application.Common;
using PropostaService.Application.Ports.Inbound;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Application.Propostas.Dtos;

namespace PropostaService.Application.Propostas.UseCases;

public sealed class ObterPropostaHandler : IObterProposta
{
    private readonly IPropostaRepository _repositorio;

    public ObterPropostaHandler(IPropostaRepository repositorio) => _repositorio = repositorio;

    public async Task<Result<PropostaResponse>> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var proposta = await _repositorio.ObterPorIdAsync(id, ct);

        return proposta is null
            ? Result.Falhar<PropostaResponse>(Erro.NaoEncontrado($"Proposta {id} não encontrada."))
            : Result.Ok(PropostaMapper.ParaResponse(proposta));
    }
}

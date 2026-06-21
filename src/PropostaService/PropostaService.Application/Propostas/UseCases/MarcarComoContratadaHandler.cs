using PropostaService.Application.Common;
using PropostaService.Application.Ports.Inbound;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Domain.Propostas.Exceptions;

namespace PropostaService.Application.Propostas.UseCases;

public sealed class MarcarComoContratadaHandler : IMarcarComoContratada
{
    private readonly IPropostaRepository _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public MarcarComoContratadaHandler(IPropostaRepository repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecutarAsync(Guid propostaId, CancellationToken ct = default)
    {
        var proposta = await _repositorio.ObterPorIdAsync(propostaId, ct);
        if (proposta is null)
        {
            return Result.Falhar(Erro.NaoEncontrado($"Proposta {propostaId} não encontrada."));
        }

        try
        {
            // Idempotente no agregado: reentrega do evento não duplica efeito.
            proposta.MarcarComoContratada();
        }
        catch (TransicaoDeStatusInvalidaException ex)
        {
            return Result.Falhar(Erro.Conflito(ex.Message));
        }

        await _unitOfWork.SalvarAlteracoesAsync(ct);
        return Result.Ok();
    }
}

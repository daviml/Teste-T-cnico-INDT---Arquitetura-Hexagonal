using PropostaService.Application.Common;
using PropostaService.Application.Ports.Inbound;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Application.Propostas.Dtos;
using PropostaService.Domain.Propostas.Exceptions;

namespace PropostaService.Application.Propostas.UseCases;

public sealed class AlterarStatusPropostaHandler : IAlterarStatusProposta
{
    private readonly IPropostaRepository _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarStatusPropostaHandler(IPropostaRepository repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecutarAsync(Guid id, AlterarStatusRequest request, CancellationToken ct = default)
    {
        var proposta = await _repositorio.ObterPorIdAsync(id, ct);
        if (proposta is null)
        {
            return Result.Falhar(Erro.NaoEncontrado($"Proposta {id} não encontrada."));
        }

        try
        {
            // O agregado é a única fonte de verdade da máquina de estados;
            // aqui apenas traduzimos a transição inválida para um Result (409).
            switch (request.Status)
            {
                case StatusPropostaDestino.Aprovada:
                    proposta.Aprovar();
                    break;
                case StatusPropostaDestino.Rejeitada:
                    proposta.Rejeitar();
                    break;
                default:
                    return Result.Falhar(Erro.Validacao($"Status de destino inválido: '{request.Status}'."));
            }
        }
        catch (TransicaoDeStatusInvalidaException ex)
        {
            return Result.Falhar(Erro.Conflito(ex.Message));
        }

        await _unitOfWork.SalvarAlteracoesAsync(ct);
        return Result.Ok();
    }
}

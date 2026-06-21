using PropostaService.Application.Common;
using PropostaService.Application.Propostas.Dtos;

namespace PropostaService.Application.Ports.Inbound;

/// <summary>Caso de uso: criar uma nova proposta.</summary>
public interface ICriarProposta
{
    Task<Result<PropostaResponse>> ExecutarAsync(CriarPropostaRequest request, CancellationToken ct = default);
}

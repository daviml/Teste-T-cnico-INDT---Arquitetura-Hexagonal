using PropostaService.Application.Common;
using PropostaService.Application.Propostas.Dtos;

namespace PropostaService.Application.Ports.Inbound;

/// <summary>Caso de uso: obter uma proposta por id.</summary>
public interface IObterProposta
{
    Task<Result<PropostaResponse>> ExecutarAsync(Guid id, CancellationToken ct = default);
}

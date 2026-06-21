using PropostaService.Application.Common;
using PropostaService.Application.Propostas.Dtos;

namespace PropostaService.Application.Ports.Inbound;

/// <summary>Caso de uso: aprovar ou rejeitar uma proposta.</summary>
public interface IAlterarStatusProposta
{
    Task<Result> ExecutarAsync(Guid id, AlterarStatusRequest request, CancellationToken ct = default);
}

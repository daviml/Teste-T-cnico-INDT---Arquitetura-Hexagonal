using ContratacaoService.Application.Common;
using ContratacaoService.Application.Contratacoes.Dtos;

namespace ContratacaoService.Application.Ports.Inbound;

/// <summary>Caso de uso: contratar uma proposta aprovada.</summary>
public interface IContratarProposta
{
    Task<Result<ContratacaoResponse>> ExecutarAsync(ContratarPropostaRequest request, CancellationToken ct = default);
}

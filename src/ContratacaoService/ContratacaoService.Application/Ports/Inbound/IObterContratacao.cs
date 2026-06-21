using ContratacaoService.Application.Common;
using ContratacaoService.Application.Contratacoes.Dtos;

namespace ContratacaoService.Application.Ports.Inbound;

/// <summary>Caso de uso: obter uma contratação por id.</summary>
public interface IObterContratacao
{
    Task<Result<ContratacaoResponse>> ExecutarAsync(Guid id, CancellationToken ct = default);
}

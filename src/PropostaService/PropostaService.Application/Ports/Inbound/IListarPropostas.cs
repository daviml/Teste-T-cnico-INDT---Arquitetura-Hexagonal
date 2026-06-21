using PropostaService.Application.Common;
using PropostaService.Application.Propostas.Dtos;

namespace PropostaService.Application.Ports.Inbound;

/// <summary>Caso de uso: listar propostas com filtro e paginação.</summary>
public interface IListarPropostas
{
    Task<Result<ResultadoPaginado<PropostaResponse>>> ExecutarAsync(
        ListarPropostasFiltro filtro,
        CancellationToken ct = default);
}

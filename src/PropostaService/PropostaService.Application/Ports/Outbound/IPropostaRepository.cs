using PropostaService.Domain.Propostas;

namespace PropostaService.Application.Ports.Outbound;

/// <summary>
/// Port de saída para persistência de propostas. A Application depende desta
/// abstração; a implementação concreta (EF Core) vive na Infrastructure.
/// </summary>
public interface IPropostaRepository
{
    Task AdicionarAsync(Proposta proposta, CancellationToken ct = default);

    Task<Proposta?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<Proposta> Itens, long Total)> ListarAsync(
        StatusProposta? status,
        string? documento,
        int pagina,
        int tamanhoPagina,
        CancellationToken ct = default);
}

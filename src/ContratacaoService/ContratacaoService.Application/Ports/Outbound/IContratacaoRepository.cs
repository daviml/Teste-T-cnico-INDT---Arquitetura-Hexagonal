using ContratacaoService.Domain.Contratacoes;

namespace ContratacaoService.Application.Ports.Outbound;

public interface IContratacaoRepository
{
    Task AdicionarAsync(Contratacao contratacao, CancellationToken ct = default);

    Task<Contratacao?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Verificação fast-fail da invariante anti-duplicação (UX).</summary>
    Task<bool> ExisteParaPropostaAsync(Guid propostaId, CancellationToken ct = default);
}

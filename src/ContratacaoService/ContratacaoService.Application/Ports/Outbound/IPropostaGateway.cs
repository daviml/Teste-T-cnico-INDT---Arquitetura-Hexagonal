using ContratacaoService.Application.Common;

namespace ContratacaoService.Application.Ports.Outbound;

/// <summary>
/// Representação interna (ACL) da proposta consultada no PropostaService. A
/// tradução do contrato externo para este modelo acontece no adapter (infra),
/// isolando a Application das mudanças do serviço remoto.
/// </summary>
public sealed record PropostaConsultada(Guid PropostaId, bool EstaAprovada, decimal ValorPremio);

/// <summary>
/// Port de saída para a consulta SÍNCRONA da proposta. O adapter (infra) cuida
/// de resiliência (Polly) e da tradução do contrato externo. O Result distingue
/// claramente "não encontrada" (404) de "indisponível" (503) — nunca confunde
/// indisponibilidade com regra violada.
/// </summary>
public interface IPropostaGateway
{
    Task<Result<PropostaConsultada>> ObterPropostaAsync(Guid propostaId, CancellationToken ct = default);
}

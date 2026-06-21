namespace ContratacaoService.Application.Contratacoes.Dtos;

/// <summary>
/// Entrada para contratar uma proposta. Recebe apenas o PropostaId — todo o
/// resto (prêmio, validade) é derivado da consulta à proposta.
/// </summary>
public sealed record ContratarPropostaRequest(Guid PropostaId);

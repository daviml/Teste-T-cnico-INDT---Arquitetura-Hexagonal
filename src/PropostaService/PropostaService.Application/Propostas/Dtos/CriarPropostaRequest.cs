namespace PropostaService.Application.Propostas.Dtos;

/// <summary>
/// Dados de entrada para criação de proposta. Recebe apenas a cobertura — o
/// prêmio é derivado por regra de domínio, NUNCA informado pelo cliente.
/// </summary>
public sealed record CriarPropostaRequest(
    string ClienteNome,
    string ClienteDocumento,
    string ClienteEmail,
    decimal ValorCobertura);

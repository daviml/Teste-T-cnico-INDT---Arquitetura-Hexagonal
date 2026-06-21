namespace ContratacaoService.Application.Contratacoes.Dtos;

/// <summary>Representação de saída de uma contratação.</summary>
public sealed record ContratacaoResponse(
    Guid Id,
    Guid PropostaId,
    string NumeroApolice,
    decimal ValorPremioPago,
    DateTimeOffset DataContratacao);

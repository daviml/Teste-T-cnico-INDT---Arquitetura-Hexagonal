namespace PropostaService.Application.Propostas.Dtos;

/// <summary>Representação de saída de uma proposta (DTO de leitura).</summary>
public sealed record PropostaResponse(
    Guid Id,
    string NumeroProposta,
    string ClienteNome,
    string ClienteDocumento,
    string TipoDocumento,
    string ClienteEmail,
    decimal ValorCobertura,
    decimal ValorPremio,
    string Moeda,
    string Status,
    DateTimeOffset DataCriacao);

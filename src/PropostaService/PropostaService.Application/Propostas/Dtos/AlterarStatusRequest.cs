namespace PropostaService.Application.Propostas.Dtos;

/// <summary>
/// Status de destino permitidos pela API ao alterar uma proposta. Restringe
/// estruturalmente a transição: Contratada só ocorre via consumo de evento,
/// nunca pela API pública.
/// </summary>
public enum StatusPropostaDestino
{
    Aprovada = 1,
    Rejeitada = 2
}

/// <summary>Dados de entrada para alteração de status de uma proposta.</summary>
public sealed record AlterarStatusRequest(StatusPropostaDestino Status);

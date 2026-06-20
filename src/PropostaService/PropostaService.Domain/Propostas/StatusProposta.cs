namespace PropostaService.Domain.Propostas;

/// <summary>
/// Estados possíveis de uma proposta. As transições válidas são controladas
/// exclusivamente pelo agregado <see cref="Proposta"/> (máquina de estados).
/// </summary>
public enum StatusProposta
{
    EmAnalise = 1,
    Aprovada = 2,
    Rejeitada = 3,
    Contratada = 4
}

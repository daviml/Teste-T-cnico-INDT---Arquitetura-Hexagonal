using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Domain.Propostas;

/// <summary>
/// Domain Service que deriva o valor do prêmio a partir da cobertura.
/// Decisão de domínio explícita: o prêmio NUNCA é informado pelo cliente —
/// é sempre calculado por esta regra.
/// </summary>
public static class CalculadoraDePremio
{
    /// <summary>Taxa base aplicada sobre a cobertura (5%).</summary>
    public const decimal TaxaBase = 0.05m;

    public static Dinheiro Calcular(Dinheiro cobertura) => cobertura.Multiplicar(TaxaBase);
}

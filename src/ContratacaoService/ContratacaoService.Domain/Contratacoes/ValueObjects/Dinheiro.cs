using ContratacaoService.Domain.Common;

namespace ContratacaoService.Domain.Contratacoes.ValueObjects;

/// <summary>
/// Value Object monetário: valor + moeda, não-negativo e arredondado a 2 casas.
/// Versão enxuta (sem aritmética) — a contratação apenas registra o valor pago.
/// </summary>
public sealed record Dinheiro
{
    public const string MoedaPadrao = "BRL";

    private Dinheiro(decimal valor, string moeda)
    {
        Valor = valor;
        Moeda = moeda;
    }

    public decimal Valor { get; }

    public string Moeda { get; }

    public static Dinheiro De(decimal valor, string moeda = MoedaPadrao)
    {
        if (valor < 0)
        {
            throw new DomainException("Valor monetário não pode ser negativo.");
        }

        if (string.IsNullOrWhiteSpace(moeda))
        {
            throw new DomainException("Moeda é obrigatória.");
        }

        return new Dinheiro(decimal.Round(valor, 2), moeda.Trim().ToUpperInvariant());
    }
}

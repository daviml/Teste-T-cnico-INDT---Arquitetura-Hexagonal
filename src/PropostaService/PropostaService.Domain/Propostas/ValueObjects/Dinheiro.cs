using PropostaService.Domain.Common;

namespace PropostaService.Domain.Propostas.ValueObjects;

/// <summary>
/// Value Object monetário: valor + moeda. Proíbe valores negativos, arredonda
/// para 2 casas e oferece operações aritméticas seguras (que rejeitam misturar
/// moedas distintas). Imutável e comparado por valor.
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

    public static Dinheiro Zero(string moeda = MoedaPadrao) => De(0m, moeda);

    public Dinheiro Multiplicar(decimal fator)
    {
        if (fator < 0)
        {
            throw new DomainException("Fator não pode ser negativo.");
        }

        return De(Valor * fator, Moeda);
    }

    public Dinheiro Somar(Dinheiro outro)
    {
        GarantirMesmaMoeda(outro);
        return De(Valor + outro.Valor, Moeda);
    }

    private void GarantirMesmaMoeda(Dinheiro outro)
    {
        if (Moeda != outro.Moeda)
        {
            throw new DomainException($"Operação inválida entre moedas distintas: {Moeda} e {outro.Moeda}.");
        }
    }
}

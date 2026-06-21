using System.Text.RegularExpressions;
using ContratacaoService.Domain.Common;

namespace ContratacaoService.Domain.Contratacoes.ValueObjects;

/// <summary>
/// Número da apólice no formato <c>APO-{ano}-{sufixo}</c>. É gerado na
/// contratação a partir do próprio Id (Guid) — autocontido, sem depender de
/// sequência externa. A unicidade é reforçada por índice único no banco.
/// </summary>
public sealed partial record NumeroApolice
{
    private const string Prefixo = "APO";

    private NumeroApolice(string valor) => Valor = valor;

    public string Valor { get; }

    public static NumeroApolice Gerar(int ano, Guid referencia)
    {
        if (ano < 2000)
        {
            throw new DomainException($"Ano inválido para número de apólice: {ano}.");
        }

        var sufixo = referencia.ToString("N")[..12].ToUpperInvariant();
        return new NumeroApolice($"{Prefixo}-{ano}-{sufixo}");
    }

    /// <summary>Reconstitui/valida um número já existente (ex.: vindo do banco).</summary>
    public static NumeroApolice De(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || !Formato().IsMatch(valor))
        {
            throw new DomainException($"Número de apólice inválido: '{valor}'.");
        }

        return new NumeroApolice(valor);
    }

    public override string ToString() => Valor;

    [GeneratedRegex(@"^APO-\d{4}-[0-9A-F]{12}$")]
    private static partial Regex Formato();
}

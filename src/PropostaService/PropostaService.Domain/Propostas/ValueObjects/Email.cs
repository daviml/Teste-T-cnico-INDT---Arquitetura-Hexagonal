using System.Text.RegularExpressions;
using PropostaService.Domain.Common;

namespace PropostaService.Domain.Propostas.ValueObjects;

/// <summary>
/// Value Object de e-mail. Valida o formato e normaliza (trim + minúsculas)
/// na construção, garantindo comparação consistente por valor.
/// </summary>
public sealed partial record Email
{
    private Email(string valor) => Valor = valor;

    public string Valor { get; }

    public static Email Criar(string? entrada)
    {
        var normalizado = (entrada ?? string.Empty).Trim().ToLowerInvariant();

        if (!FormatoEmail().IsMatch(normalizado))
        {
            throw new DomainException($"E-mail inválido: '{entrada}'.");
        }

        return new Email(normalizado);
    }

    public override string ToString() => Valor;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex FormatoEmail();
}

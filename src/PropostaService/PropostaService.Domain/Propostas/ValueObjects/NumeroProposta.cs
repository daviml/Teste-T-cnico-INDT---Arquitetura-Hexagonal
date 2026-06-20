using System.Text.RegularExpressions;
using PropostaService.Domain.Common;

namespace PropostaService.Domain.Propostas.ValueObjects;

/// <summary>
/// Código amigável da proposta no formato <c>PRP-{ano}-{sequencial}</c>.
/// A regra de formatação vive no domínio; a <em>origem</em> do sequencial
/// (sequência do banco) é uma preocupação de infraestrutura — por isso é
/// recebido como parâmetro, e não gerado aqui.
/// </summary>
public sealed partial record NumeroProposta
{
    private const string Prefixo = "PRP";

    private NumeroProposta(string valor) => Valor = valor;

    public string Valor { get; }

    public static NumeroProposta Gerar(int ano, long sequencial)
    {
        if (ano < 2000)
        {
            throw new DomainException($"Ano inválido para número de proposta: {ano}.");
        }

        if (sequencial <= 0)
        {
            throw new DomainException($"Sequencial deve ser positivo: {sequencial}.");
        }

        return new NumeroProposta($"{Prefixo}-{ano}-{sequencial:D6}");
    }

    /// <summary>Reconstitui/valida um número já existente (ex.: vindo do banco).</summary>
    public static NumeroProposta De(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || !Formato().IsMatch(valor))
        {
            throw new DomainException($"Número de proposta inválido: '{valor}'.");
        }

        return new NumeroProposta(valor);
    }

    public override string ToString() => Valor;

    [GeneratedRegex(@"^PRP-\d{4}-\d{6,}$")]
    private static partial Regex Formato();
}

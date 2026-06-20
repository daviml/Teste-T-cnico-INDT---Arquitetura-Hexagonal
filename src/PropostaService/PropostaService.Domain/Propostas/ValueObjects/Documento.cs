using System.Linq;
using PropostaService.Domain.Common;

namespace PropostaService.Domain.Propostas.ValueObjects;

public enum TipoDocumento
{
    Cpf,
    Cnpj
}

/// <summary>
/// Value Object que encapsula CPF ou CNPJ. Valida formato e dígitos
/// verificadores <em>na construção</em> — uma instância só existe se for
/// válida (invariante <em>by construction</em>). Armazena somente os dígitos.
/// </summary>
public sealed record Documento
{
    private static readonly int[] PesosCnpjPrimeiro = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] PesosCnpjSegundo = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private Documento(string valor, TipoDocumento tipo)
    {
        Valor = valor;
        Tipo = tipo;
    }

    /// <summary>Apenas os dígitos (sem máscara).</summary>
    public string Valor { get; }

    public TipoDocumento Tipo { get; }

    public static Documento Criar(string? entrada)
    {
        var digitos = SomenteDigitos(entrada);

        return digitos.Length switch
        {
            11 when CpfValido(digitos) => new Documento(digitos, TipoDocumento.Cpf),
            14 when CnpjValido(digitos) => new Documento(digitos, TipoDocumento.Cnpj),
            _ => throw new DomainException($"Documento inválido: '{entrada}'.")
        };
    }

    private static string SomenteDigitos(string? entrada) =>
        new([.. (entrada ?? string.Empty).Where(char.IsDigit)]);

    private static bool CpfValido(string cpf)
    {
        if (cpf.All(c => c == cpf[0]))
        {
            return false;
        }

        var dv1 = DigitoModulo11Sequencial(cpf[..9]);
        var dv2 = DigitoModulo11Sequencial(cpf[..10]);
        return dv1 == cpf[9] - '0' && dv2 == cpf[10] - '0';
    }

    private static bool CnpjValido(string cnpj)
    {
        if (cnpj.All(c => c == cnpj[0]))
        {
            return false;
        }

        var dv1 = DigitoModulo11Pesos(cnpj[..12], PesosCnpjPrimeiro);
        var dv2 = DigitoModulo11Pesos(cnpj[..13], PesosCnpjSegundo);
        return dv1 == cnpj[12] - '0' && dv2 == cnpj[13] - '0';
    }

    // CPF: pesos decrescentes começando em (tamanho + 1).
    private static int DigitoModulo11Sequencial(string baseDigitos)
    {
        var soma = 0;
        var peso = baseDigitos.Length + 1;
        foreach (var c in baseDigitos)
        {
            soma += (c - '0') * peso--;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    // CNPJ: pesos fixos por posição.
    private static int DigitoModulo11Pesos(string baseDigitos, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < baseDigitos.Length; i++)
        {
            soma += (baseDigitos[i] - '0') * pesos[i];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}

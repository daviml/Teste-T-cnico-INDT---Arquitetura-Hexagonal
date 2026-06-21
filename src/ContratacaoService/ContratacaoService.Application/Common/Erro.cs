namespace ContratacaoService.Application.Common;

/// <summary>
/// Classifica a natureza de uma falha. A API traduz cada tipo para o status
/// HTTP adequado. Note <see cref="Indisponivel"/> (503): indisponibilidade de
/// dependência NÃO é a mesma coisa que regra de negócio violada (409/400).
/// </summary>
public enum TipoErro
{
    Falha = 0,
    Validacao = 1,
    NaoEncontrado = 2,
    Conflito = 3,
    Indisponivel = 4
}

public sealed record Erro(string Codigo, string Mensagem, TipoErro Tipo)
{
    public static readonly Erro Nenhum = new(string.Empty, string.Empty, TipoErro.Falha);

    public static Erro Validacao(string mensagem) => new("validacao", mensagem, TipoErro.Validacao);

    public static Erro NaoEncontrado(string mensagem) => new("nao_encontrado", mensagem, TipoErro.NaoEncontrado);

    public static Erro Conflito(string mensagem) => new("conflito", mensagem, TipoErro.Conflito);

    public static Erro Indisponivel(string mensagem) => new("indisponivel", mensagem, TipoErro.Indisponivel);
}

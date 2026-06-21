namespace ContratacaoService.Application.Common;

/// <summary>
/// Result pattern: sucesso ou falha sem usar exceções para controle de fluxo.
/// </summary>
public class Result
{
    protected Result(bool sucesso, Erro erro)
    {
        if (sucesso && erro != Erro.Nenhum)
        {
            throw new InvalidOperationException("Um resultado de sucesso não pode carregar um erro.");
        }

        if (!sucesso && erro == Erro.Nenhum)
        {
            throw new InvalidOperationException("Um resultado de falha exige um erro.");
        }

        Sucesso = sucesso;
        Erro = erro;
    }

    public bool Sucesso { get; }

    public bool Falhou => !Sucesso;

    public Erro Erro { get; }

    public static Result Ok() => new(true, Erro.Nenhum);

    public static Result Falhar(Erro erro) => new(false, erro);

    public static Result<T> Ok<T>(T valor) => new(valor, true, Erro.Nenhum);

    public static Result<T> Falhar<T>(Erro erro) => new(default, false, erro);
}

/// <summary>Result que carrega um valor em caso de sucesso.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _valor;

    internal Result(T? valor, bool sucesso, Erro erro) : base(sucesso, erro) => _valor = valor;

    public T Valor => Sucesso
        ? _valor!
        : throw new InvalidOperationException("Não há valor disponível em um resultado de falha.");
}

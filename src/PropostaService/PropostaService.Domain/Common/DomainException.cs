namespace PropostaService.Domain.Common;

/// <summary>
/// Sinaliza a violação de uma invariante de domínio — um estado que jamais
/// deveria ocorrer. Regras de negócio <em>esperadas</em> (ex.: proposta não
/// aprovada) são tratadas com Result na camada de aplicação; exceções de
/// domínio NÃO são usadas para controle de fluxo.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem)
    {
    }
}

namespace ContratacaoService.Domain.Common;

/// <summary>
/// Sinaliza a violação de uma invariante de domínio. Regras de negócio
/// esperadas são tratadas com Result na camada de aplicação; exceções de
/// domínio NÃO são usadas para controle de fluxo.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem)
    {
    }
}

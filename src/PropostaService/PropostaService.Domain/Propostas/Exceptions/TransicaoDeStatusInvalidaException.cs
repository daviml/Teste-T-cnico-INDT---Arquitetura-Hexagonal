using PropostaService.Domain.Common;

namespace PropostaService.Domain.Propostas.Exceptions;

/// <summary>
/// Lançada quando se tenta uma transição de status não permitida pela máquina
/// de estados da proposta (ex.: aprovar uma proposta já rejeitada).
/// </summary>
public sealed class TransicaoDeStatusInvalidaException : DomainException
{
    public TransicaoDeStatusInvalidaException(StatusProposta statusAtual, string acao)
        : base($"Transição inválida: não é possível {acao} uma proposta com status '{statusAtual}'.")
    {
    }
}

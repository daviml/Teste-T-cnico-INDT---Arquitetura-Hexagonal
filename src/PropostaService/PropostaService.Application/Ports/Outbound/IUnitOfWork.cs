namespace PropostaService.Application.Ports.Outbound;

/// <summary>
/// Port de saída que confirma (commita) as alterações pendentes numa única
/// transação. Separa a intenção (repositório agrega/altera) do commit.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SalvarAlteracoesAsync(CancellationToken ct = default);
}

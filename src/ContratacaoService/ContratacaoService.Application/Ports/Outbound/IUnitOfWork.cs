namespace ContratacaoService.Application.Ports.Outbound;

/// <summary>Confirma as alterações pendentes numa única transação.</summary>
public interface IUnitOfWork
{
    Task<int> SalvarAlteracoesAsync(CancellationToken ct = default);
}

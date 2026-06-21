using ContratacaoService.Domain.Contratacoes.Events;

namespace ContratacaoService.Application.Ports.Outbound;

/// <summary>
/// Port de saída para publicação de eventos. Recebe o evento de DOMÍNIO; o
/// adapter (infra) traduz para o integration event (ContratacaoRealizada) e o
/// grava no Outbox, na mesma transação do SalvarAlteracoes.
/// </summary>
public interface IEventBus
{
    Task PublicarAsync(ContratacaoEfetuadaEvent evento, CancellationToken ct = default);
}

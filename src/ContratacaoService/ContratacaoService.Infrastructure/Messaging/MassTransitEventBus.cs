using Contracts.IntegrationEvents.V1;
using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.Contratacoes.Events;
using MassTransit;

namespace ContratacaoService.Infrastructure.Messaging;

/// <summary>
/// Adapter de saída para mensageria. Faz a tradução (ACL) do evento de DOMÍNIO
/// <see cref="ContratacaoEfetuadaEvent"/> para o integration event
/// <see cref="ContratacaoRealizada"/> — entidades de domínio nunca vão à fila.
///
/// O <see cref="IPublishEndpoint"/> aqui é o do <b>Bus Outbox</b> do MassTransit
/// (configurado com <c>UseBusOutbox()</c>): o Publish não vai direto ao broker —
/// é gravado na tabela de Outbox via o mesmo <c>DbContext</c> do escopo. Como o
/// handler chama este método ANTES de <c>SalvarAlteracoesAsync</c>, o evento é
/// persistido na MESMA transação do agregado. Um serviço em background entrega
/// ao RabbitMQ depois (resiliente a broker indisponível).
/// </summary>
internal sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

    public Task PublicarAsync(ContratacaoEfetuadaEvent evento, CancellationToken ct = default)
    {
        var integrationEvent = new ContratacaoRealizada
        {
            ContratacaoId = evento.ContratacaoId,
            PropostaId = evento.PropostaId,
            NumeroApolice = evento.NumeroApolice,
            ValorPremioPago = evento.ValorPremioPago,
            OcorridaEm = evento.OcorridoEm,
        };

        return _publishEndpoint.Publish(integrationEvent, ct);
    }
}

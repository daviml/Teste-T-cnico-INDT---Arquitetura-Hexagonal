using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.Contratacoes.Events;
using Microsoft.Extensions.Logging;

namespace ContratacaoService.Infrastructure.Messaging;

/// <summary>
/// Implementação PLACEHOLDER do barramento de eventos: apenas registra em log.
/// Permite o serviço operar antes da mensageria. Será substituída no Commit 12
/// pela publicação via Transactional Outbox (MassTransit + RabbitMQ).
/// </summary>
internal sealed partial class LoggingEventBus : IEventBus
{
    private readonly ILogger<LoggingEventBus> _logger;

    public LoggingEventBus(ILogger<LoggingEventBus> logger) => _logger = logger;

    public Task PublicarAsync(ContratacaoEfetuadaEvent evento, CancellationToken ct = default)
    {
        LogEventoPublicado(evento.ContratacaoId, evento.PropostaId, evento.NumeroApolice);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Evento ContratacaoEfetuada (placeholder): Contratacao={ContratacaoId} Proposta={PropostaId} Apolice={NumeroApolice}")]
    private partial void LogEventoPublicado(Guid contratacaoId, Guid propostaId, string numeroApolice);
}

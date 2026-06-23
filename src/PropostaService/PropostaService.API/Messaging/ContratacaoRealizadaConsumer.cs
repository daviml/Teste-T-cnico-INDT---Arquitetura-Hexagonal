using Contracts.IntegrationEvents.V1;
using MassTransit;
using Microsoft.Extensions.Logging;
using PropostaService.Application.Common;
using PropostaService.Application.Ports.Inbound;

namespace PropostaService.API.Messaging;

/// <summary>
/// Driving adapter de mensageria: consome o integration event
/// <see cref="ContratacaoRealizada"/> e aciona o caso de uso
/// <see cref="IMarcarComoContratada"/>.
///
/// Idempotência em duas camadas (defesa em profundidade):
/// <list type="number">
///   <item>Inbox do MassTransit — descarta reentregas (mesmo MessageId) antes de
///   chamar o consumer, na transação do DbContext.</item>
///   <item>Agregado Proposta — <c>MarcarComoContratada</c> é no-op se já estiver
///   Contratada, caso a mensagem escape do Inbox (ex.: contexto diferente).</item>
/// </list>
/// </summary>
public sealed partial class ContratacaoRealizadaConsumer : IConsumer<ContratacaoRealizada>
{
    private readonly IMarcarComoContratada _marcarComoContratada;
    private readonly ILogger<ContratacaoRealizadaConsumer> _logger;

    public ContratacaoRealizadaConsumer(
        IMarcarComoContratada marcarComoContratada,
        ILogger<ContratacaoRealizadaConsumer> logger)
    {
        _marcarComoContratada = marcarComoContratada;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ContratacaoRealizada> context)
    {
        var propostaId = context.Message.PropostaId;

        var resultado = await _marcarComoContratada.ExecutarAsync(propostaId, context.CancellationToken);
        if (resultado.Falhou)
        {
            LogFalha(propostaId, resultado.Erro.Tipo, resultado.Erro.Mensagem);

            // Não engolimos a falha: lançar leva a mensagem à fila de erro do broker
            // (sem perda silenciosa), para investigação/reprocessamento manual.
            throw new InvalidOperationException(
                $"Falha ao marcar proposta {propostaId} como contratada: {resultado.Erro.Mensagem}");
        }

        LogContratada(propostaId, context.Message.NumeroApolice);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Proposta {PropostaId} marcada como contratada (apólice {NumeroApolice}).")]
    private partial void LogContratada(Guid propostaId, string numeroApolice);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Falha ao processar ContratacaoRealizada para proposta {PropostaId}: {Tipo} - {Mensagem}")]
    private partial void LogFalha(Guid propostaId, TipoErro tipo, string mensagem);
}

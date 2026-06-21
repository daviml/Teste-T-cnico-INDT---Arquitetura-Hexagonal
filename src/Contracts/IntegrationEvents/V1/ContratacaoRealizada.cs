namespace Contracts.IntegrationEvents.V1;

/// <summary>
/// Integration event publicado pelo ContratacaoService quando uma contratação é
/// efetuada, e consumido pelo PropostaService para marcar a proposta como contratada.
///
/// CONTRATO ESTÁVEL entre serviços. O par (namespace + nome) identifica a mensagem
/// no broker — o MassTransit deriva o message-urn daqui. Por isso:
/// <list type="bullet">
///   <item>Não renomear o tipo nem mover de namespace.</item>
///   <item>Evolução compatível (adicionar campo opcional) é permitida; mudança
///   incompatível cria uma nova versão (ex.: <c>...V2.ContratacaoRealizada</c>),
///   nunca altera esta.</item>
///   <item>Apenas tipos primitivos/serializáveis — nunca entidades de domínio
///   ou Value Objects (a tradução domain event → integration event acontece na
///   borda da Infrastructure do ContratacaoService).</item>
/// </list>
/// </summary>
public sealed record ContratacaoRealizada
{
    /// <summary>Identificador da contratação no ContratacaoService (rastreabilidade).</summary>
    public required Guid ContratacaoId { get; init; }

    /// <summary>Proposta contratada — chave usada pelo PropostaService para localizar o agregado.</summary>
    public required Guid PropostaId { get; init; }

    /// <summary>Número da apólice gerado na contratação.</summary>
    public required string NumeroApolice { get; init; }

    /// <summary>Prêmio efetivamente pago na contratação.</summary>
    public required decimal ValorPremioPago { get; init; }

    /// <summary>Momento em que a contratação ocorreu (UTC).</summary>
    public required DateTimeOffset OcorridaEm { get; init; }
}

using ContratacaoService.Domain.Common;

namespace ContratacaoService.Domain.Contratacoes.Events;

/// <summary>
/// Disparado quando uma contratação é efetuada. Evento de domínio INTERNO —
/// será traduzido para o integration event (ContratacaoRealizada) na borda da
/// infraestrutura, que é o contrato publicado para o PropostaService.
/// </summary>
public sealed record ContratacaoEfetuadaEvent(
    Guid ContratacaoId,
    Guid PropostaId,
    string NumeroApolice,
    decimal ValorPremioPago) : IDomainEvent
{
    public DateTimeOffset OcorridoEm { get; init; } = DateTimeOffset.UtcNow;
}

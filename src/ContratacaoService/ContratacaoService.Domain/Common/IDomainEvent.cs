namespace ContratacaoService.Domain.Common;

/// <summary>
/// Marcador para eventos de domínio. A tradução para integration events
/// (contrato entre serviços) acontece na borda da infraestrutura.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OcorridoEm { get; }
}

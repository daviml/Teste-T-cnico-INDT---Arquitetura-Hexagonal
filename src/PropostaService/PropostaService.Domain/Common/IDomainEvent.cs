namespace PropostaService.Domain.Common;

/// <summary>
/// Marcador para eventos de domínio — fatos relevantes ocorridos dentro do
/// agregado. Vivem no domínio; a tradução para <em>integration events</em>
/// (contrato entre serviços) acontece na borda da infraestrutura.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OcorridoEm { get; }
}

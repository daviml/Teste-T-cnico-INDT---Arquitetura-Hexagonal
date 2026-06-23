namespace ContratacaoService.Infrastructure.Observability;

/// <summary>
/// Portador (scoped) do correlation id da requisição atual. Preenchido pelo
/// middleware na borda da API e lido pelo adapter de mensageria para propagar
/// o id na publicação do evento — sem acoplar a Infrastructure ao HttpContext.
/// </summary>
public sealed class CorrelationContext
{
    public string CorrelationId { get; set; } = string.Empty;
}

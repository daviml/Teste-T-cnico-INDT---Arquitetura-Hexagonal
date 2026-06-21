namespace PropostaService.Application.Propostas.Dtos;

/// <summary>
/// Filtros de listagem (contrato externo). O <see cref="Status"/> chega como
/// texto e é validado/convertido no handler — o enum de domínio não vaza para
/// o contrato de entrada.
/// </summary>
public sealed record ListarPropostasFiltro
{
    public string? Status { get; init; }

    public string? Documento { get; init; }

    public int Pagina { get; init; } = 1;

    public int TamanhoPagina { get; init; } = 20;
}

namespace PropostaService.Application.Common;

/// <summary>Página de resultados de uma consulta, com o total geral de itens.</summary>
public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    long Total);

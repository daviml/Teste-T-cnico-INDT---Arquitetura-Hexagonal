namespace Contracts;

/// <summary>
/// Convenção compartilhada do header de correlação. Único ponto de verdade do
/// NOME do header, usado tanto no HTTP (request/response) quanto na propagação
/// pela fila (publisher seta, consumer lê) — garante que os dois lados batam.
/// </summary>
public static class CorrelationHeader
{
    public const string Name = "X-Correlation-ID";
}

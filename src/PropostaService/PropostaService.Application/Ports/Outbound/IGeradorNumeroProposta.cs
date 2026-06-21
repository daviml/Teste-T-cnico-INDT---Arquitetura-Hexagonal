using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Application.Ports.Outbound;

/// <summary>
/// Port de saída que gera o próximo número de proposta. A ORIGEM do sequencial
/// (sequência do banco) é uma preocupação de infraestrutura — encapsulada aqui
/// para manter a Application e o domínio agnósticos.
/// </summary>
public interface IGeradorNumeroProposta
{
    Task<NumeroProposta> GerarProximoAsync(CancellationToken ct = default);
}

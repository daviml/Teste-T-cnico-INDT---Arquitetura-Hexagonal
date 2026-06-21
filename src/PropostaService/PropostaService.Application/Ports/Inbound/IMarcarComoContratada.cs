using PropostaService.Application.Common;

namespace PropostaService.Application.Ports.Inbound;

/// <summary>
/// Caso de uso: marcar uma proposta como contratada. Acionado EXCLUSIVAMENTE
/// pelo consumer do evento de contratação (driving adapter de mensageria),
/// nunca pela API REST pública. Idempotente.
/// </summary>
public interface IMarcarComoContratada
{
    Task<Result> ExecutarAsync(Guid propostaId, CancellationToken ct = default);
}

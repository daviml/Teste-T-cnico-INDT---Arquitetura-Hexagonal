using PropostaService.Domain.Common;

namespace PropostaService.Domain.Propostas.Events;

/// <summary>Disparado quando uma nova proposta é criada (status inicial EmAnalise).</summary>
public sealed record PropostaCriadaEvent(Guid PropostaId, string NumeroProposta) : IDomainEvent
{
    public DateTimeOffset OcorridoEm { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>Disparado quando uma proposta é aprovada.</summary>
public sealed record PropostaAprovadaEvent(Guid PropostaId) : IDomainEvent
{
    public DateTimeOffset OcorridoEm { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>Disparado quando uma proposta é rejeitada.</summary>
public sealed record PropostaRejeitadaEvent(Guid PropostaId) : IDomainEvent
{
    public DateTimeOffset OcorridoEm { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>Disparado quando uma proposta passa a Contratada (via consumo de evento).</summary>
public sealed record PropostaContratadaEvent(Guid PropostaId) : IDomainEvent
{
    public DateTimeOffset OcorridoEm { get; init; } = DateTimeOffset.UtcNow;
}

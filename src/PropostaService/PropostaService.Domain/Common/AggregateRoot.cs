namespace PropostaService.Domain.Common;

/// <summary>
/// Raiz de agregado: carrega a identidade e acumula os eventos de domínio
/// disparados durante as transições de estado. Os eventos são drenados
/// (lidos e limpos) pela infraestrutura após a persistência.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _eventos = [];

    protected AggregateRoot(Guid id) => Id = id;

    public Guid Id { get; private set; }

    public IReadOnlyCollection<IDomainEvent> EventosDeDominio => _eventos.AsReadOnly();

    protected void RegistrarEvento(IDomainEvent evento) => _eventos.Add(evento);

    public void LimparEventos() => _eventos.Clear();
}

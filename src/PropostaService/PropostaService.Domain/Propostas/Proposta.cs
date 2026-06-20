using PropostaService.Domain.Common;
using PropostaService.Domain.Propostas.Events;
using PropostaService.Domain.Propostas.Exceptions;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Domain.Propostas;

/// <summary>
/// Raiz de agregado Proposta. Encapsula estado e transições: não há setters
/// públicos de <see cref="Status"/>, e toda mudança passa pela máquina de
/// estados abaixo.
/// <code>
/// EmAnalise → Aprovada  → Contratada (somente via evento)
///           → Rejeitada
/// </code>
/// </summary>
public sealed class Proposta : AggregateRoot
{
    private Proposta(
        Guid id,
        NumeroProposta numero,
        string clienteNome,
        Documento clienteDocumento,
        Email clienteEmail,
        Dinheiro valorCobertura,
        Dinheiro valorPremio,
        DateTimeOffset dataCriacao) : base(id)
    {
        Numero = numero;
        ClienteNome = clienteNome;
        ClienteDocumento = clienteDocumento;
        ClienteEmail = clienteEmail;
        ValorCobertura = valorCobertura;
        ValorPremio = valorPremio;
        Status = StatusProposta.EmAnalise;
        DataCriacao = dataCriacao;
    }

    public NumeroProposta Numero { get; private set; }

    public string ClienteNome { get; private set; }

    public Documento ClienteDocumento { get; private set; }

    public Email ClienteEmail { get; private set; }

    public Dinheiro ValorCobertura { get; private set; }

    public Dinheiro ValorPremio { get; private set; }

    public StatusProposta Status { get; private set; }

    public DateTimeOffset DataCriacao { get; private set; }

    /// <summary>
    /// Factory de criação. Calcula o prêmio por regra de domínio, inicia em
    /// <see cref="StatusProposta.EmAnalise"/> e registra o evento de criação.
    /// </summary>
    public static Proposta Criar(
        NumeroProposta numero,
        string clienteNome,
        Documento clienteDocumento,
        Email clienteEmail,
        Dinheiro valorCobertura)
    {
        if (string.IsNullOrWhiteSpace(clienteNome))
        {
            throw new DomainException("Nome do cliente é obrigatório.");
        }

        if (valorCobertura.Valor <= 0)
        {
            throw new DomainException("Valor de cobertura deve ser positivo.");
        }

        var valorPremio = CalculadoraDePremio.Calcular(valorCobertura);

        var proposta = new Proposta(
            Guid.NewGuid(),
            numero,
            clienteNome.Trim(),
            clienteDocumento,
            clienteEmail,
            valorCobertura,
            valorPremio,
            DateTimeOffset.UtcNow);

        proposta.RegistrarEvento(new PropostaCriadaEvent(proposta.Id, numero.Valor));
        return proposta;
    }

    public void Aprovar()
    {
        if (Status != StatusProposta.EmAnalise)
        {
            throw new TransicaoDeStatusInvalidaException(Status, "aprovar");
        }

        Status = StatusProposta.Aprovada;
        RegistrarEvento(new PropostaAprovadaEvent(Id));
    }

    public void Rejeitar()
    {
        if (Status != StatusProposta.EmAnalise)
        {
            throw new TransicaoDeStatusInvalidaException(Status, "rejeitar");
        }

        Status = StatusProposta.Rejeitada;
        RegistrarEvento(new PropostaRejeitadaEvent(Id));
    }

    /// <summary>
    /// Marca a proposta como contratada. Chamado EXCLUSIVAMENTE pelo consumer
    /// do evento de contratação (nunca pela API pública). É idempotente: se já
    /// estiver Contratada, é no-op — protege contra reentrega de mensagens.
    /// </summary>
    public void MarcarComoContratada()
    {
        if (Status == StatusProposta.Contratada)
        {
            return;
        }

        if (Status != StatusProposta.Aprovada)
        {
            throw new TransicaoDeStatusInvalidaException(Status, "contratar");
        }

        Status = StatusProposta.Contratada;
        RegistrarEvento(new PropostaContratadaEvent(Id));
    }
}

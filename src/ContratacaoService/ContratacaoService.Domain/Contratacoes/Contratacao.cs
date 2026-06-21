using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Contratacoes.Events;
using ContratacaoService.Domain.Contratacoes.ValueObjects;

namespace ContratacaoService.Domain.Contratacoes;

/// <summary>
/// Raiz de agregado Contratacao. É criada uma única vez (via <see cref="Efetuar"/>)
/// e é imutável depois — não há máquina de estados. A invariante "uma proposta
/// só pode ser contratada uma vez" NÃO é responsabilidade deste agregado (ele
/// não enxerga as outras contratações): é garantida na Application (fast-fail)
/// e pelo índice único de PropostaId no banco (fonte de verdade).
/// </summary>
public sealed class Contratacao : AggregateRoot
{
    private Contratacao(
        Guid id,
        Guid propostaId,
        NumeroApolice numeroApolice,
        Dinheiro valorPremioPago,
        DateTimeOffset dataContratacao) : base(id)
    {
        PropostaId = propostaId;
        NumeroApolice = numeroApolice;
        ValorPremioPago = valorPremioPago;
        DataContratacao = dataContratacao;
    }

    public Guid PropostaId { get; private set; }

    public NumeroApolice NumeroApolice { get; private set; }

    public Dinheiro ValorPremioPago { get; private set; }

    public DateTimeOffset DataContratacao { get; private set; }

    /// <summary>
    /// Efetua a contratação de uma proposta: gera a apólice e a data, e registra
    /// o evento de domínio. O prêmio vem da proposta (obtido via gateway na
    /// Application), nunca informado diretamente pelo cliente.
    /// </summary>
    public static Contratacao Efetuar(Guid propostaId, Dinheiro valorPremio)
    {
        if (propostaId == Guid.Empty)
        {
            throw new DomainException("PropostaId é obrigatório para efetuar a contratação.");
        }

        if (valorPremio.Valor <= 0)
        {
            throw new DomainException("Valor do prêmio deve ser positivo.");
        }

        var id = Guid.NewGuid();
        var dataContratacao = DateTimeOffset.UtcNow;
        var numeroApolice = NumeroApolice.Gerar(dataContratacao.Year, id);

        var contratacao = new Contratacao(id, propostaId, numeroApolice, valorPremio, dataContratacao);

        contratacao.RegistrarEvento(new ContratacaoEfetuadaEvent(
            id,
            propostaId,
            numeroApolice.Valor,
            valorPremio.Valor));

        return contratacao;
    }
}

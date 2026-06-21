using ContratacaoService.Application.Common;
using ContratacaoService.Application.Contratacoes.Dtos;
using ContratacaoService.Application.Ports.Inbound;
using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.Common;
using ContratacaoService.Domain.Contratacoes;
using ContratacaoService.Domain.Contratacoes.Events;
using ContratacaoService.Domain.Contratacoes.ValueObjects;

namespace ContratacaoService.Application.Contratacoes.UseCases;

public sealed class ContratarPropostaHandler : IContratarProposta
{
    private readonly IContratacaoRepository _repositorio;
    private readonly IPropostaGateway _propostaGateway;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;

    public ContratarPropostaHandler(
        IContratacaoRepository repositorio,
        IPropostaGateway propostaGateway,
        IEventBus eventBus,
        IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _propostaGateway = propostaGateway;
        _eventBus = eventBus;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ContratacaoResponse>> ExecutarAsync(ContratarPropostaRequest request, CancellationToken ct = default)
    {
        // 1. Fast-fail local: a proposta já foi contratada? (verdade final = índice único)
        if (await _repositorio.ExisteParaPropostaAsync(request.PropostaId, ct))
        {
            return Result.Falhar<ContratacaoResponse>(
                Erro.Conflito($"A proposta {request.PropostaId} já foi contratada."));
        }

        // 2. Consulta síncrona da proposta. Indisponibilidade (503) e inexistência
        //    (404) já vêm tipadas do gateway — apenas propagamos.
        var consulta = await _propostaGateway.ObterPropostaAsync(request.PropostaId, ct);
        if (consulta.Falhou)
        {
            return Result.Falhar<ContratacaoResponse>(consulta.Erro);
        }

        var proposta = consulta.Valor;
        if (!proposta.EstaAprovada)
        {
            return Result.Falhar<ContratacaoResponse>(
                Erro.Conflito("A proposta não está aprovada e não pode ser contratada."));
        }

        // 3. Efetua a contratação no domínio.
        Contratacao contratacao;
        try
        {
            contratacao = Contratacao.Efetuar(request.PropostaId, Dinheiro.De(proposta.ValorPremio));
        }
        catch (DomainException ex)
        {
            return Result.Falhar<ContratacaoResponse>(Erro.Validacao(ex.Message));
        }

        // 4. Persiste e publica o evento na MESMA transação (Outbox na infra).
        await _repositorio.AdicionarAsync(contratacao, ct);

        var evento = contratacao.EventosDeDominio.OfType<ContratacaoEfetuadaEvent>().Single();
        await _eventBus.PublicarAsync(evento, ct);

        await _unitOfWork.SalvarAlteracoesAsync(ct);

        return Result.Ok(ContratacaoMapper.ParaResponse(contratacao));
    }
}

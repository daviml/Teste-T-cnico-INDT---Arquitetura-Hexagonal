using PropostaService.Application.Common;
using PropostaService.Application.Ports.Inbound;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Application.Propostas.Dtos;
using PropostaService.Domain.Common;
using PropostaService.Domain.Propostas;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Application.Propostas.UseCases;

public sealed class CriarPropostaHandler : ICriarProposta
{
    private readonly IPropostaRepository _repositorio;
    private readonly IGeradorNumeroProposta _gerador;
    private readonly IUnitOfWork _unitOfWork;

    public CriarPropostaHandler(
        IPropostaRepository repositorio,
        IGeradorNumeroProposta gerador,
        IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _gerador = gerador;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PropostaResponse>> ExecutarAsync(CriarPropostaRequest request, CancellationToken ct = default)
    {
        try
        {
            // VOs validam a entrada na construção; só consumimos um número de
            // proposta depois que documento/e-mail/cobertura já são válidos.
            var documento = Documento.Criar(request.ClienteDocumento);
            var email = Email.Criar(request.ClienteEmail);
            var cobertura = Dinheiro.De(request.ValorCobertura);

            var numero = await _gerador.GerarProximoAsync(ct);
            var proposta = Proposta.Criar(numero, request.ClienteNome, documento, email, cobertura);

            await _repositorio.AdicionarAsync(proposta, ct);
            await _unitOfWork.SalvarAlteracoesAsync(ct);

            return Result.Ok(PropostaMapper.ParaResponse(proposta));
        }
        catch (DomainException ex)
        {
            // Entrada inválida é uma falha esperada → vira Result (mapeado a 400),
            // não uma exceção que vaza para a borda.
            return Result.Falhar<PropostaResponse>(Erro.Validacao(ex.Message));
        }
    }
}

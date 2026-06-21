using ContratacaoService.Application.Contratacoes.Dtos;
using ContratacaoService.Domain.Contratacoes;

namespace ContratacaoService.Application.Contratacoes;

/// <summary>Mapeamento explícito (manual) entre o agregado e o DTO de saída.</summary>
internal static class ContratacaoMapper
{
    public static ContratacaoResponse ParaResponse(Contratacao contratacao) => new(
        contratacao.Id,
        contratacao.PropostaId,
        contratacao.NumeroApolice.Valor,
        contratacao.ValorPremioPago.Valor,
        contratacao.DataContratacao);
}

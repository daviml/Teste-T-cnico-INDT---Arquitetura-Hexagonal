using PropostaService.Application.Propostas.Dtos;
using PropostaService.Domain.Propostas;

namespace PropostaService.Application.Propostas;

/// <summary>
/// Mapeamento explícito entre o agregado de domínio e o DTO de saída.
/// Manual e por decisão: sem AutoMapper, evitando surpresas em runtime.
/// </summary>
internal static class PropostaMapper
{
    public static PropostaResponse ParaResponse(Proposta proposta) => new(
        proposta.Id,
        proposta.Numero.Valor,
        proposta.ClienteNome,
        proposta.ClienteDocumento.Valor,
        proposta.ClienteDocumento.Tipo.ToString(),
        proposta.ClienteEmail.Valor,
        proposta.ValorCobertura.Valor,
        proposta.ValorPremio.Valor,
        proposta.ValorCobertura.Moeda,
        proposta.Status.ToString(),
        proposta.DataCriacao);
}

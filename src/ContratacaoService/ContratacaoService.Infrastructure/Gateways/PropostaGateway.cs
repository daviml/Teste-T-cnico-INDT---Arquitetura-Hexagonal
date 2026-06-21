using System.Net;
using System.Net.Http.Json;
using ContratacaoService.Application.Common;
using ContratacaoService.Application.Ports.Outbound;
using Polly;

namespace ContratacaoService.Infrastructure.Gateways;

/// <summary>
/// Adapter HTTP para o PropostaService. A resiliência (timeout/retry/circuit
/// breaker) é aplicada por fora, via pipeline configurado na DI. Aqui mora a
/// ACL: traduz o contrato externo para o modelo interno e mapeia falhas de
/// DISPONIBILIDADE (503) separadamente de "não encontrada" (404).
/// </summary>
internal sealed class PropostaGateway : IPropostaGateway
{
    private readonly HttpClient _httpClient;

    public PropostaGateway(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Result<PropostaConsultada>> ObterPropostaAsync(Guid propostaId, CancellationToken ct = default)
    {
        try
        {
            using var resposta = await _httpClient.GetAsync($"api/v1/propostas/{propostaId}", ct);

            if (resposta.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Falhar<PropostaConsultada>(
                    Erro.NaoEncontrado($"Proposta {propostaId} não encontrada no PropostaService."));
            }

            if (!resposta.IsSuccessStatusCode)
            {
                return Result.Falhar<PropostaConsultada>(
                    Erro.Indisponivel($"PropostaService respondeu HTTP {(int)resposta.StatusCode}."));
            }

            var externa = await resposta.Content.ReadFromJsonAsync<PropostaExterna>(ct);
            if (externa is null)
            {
                return Result.Falhar<PropostaConsultada>(
                    Erro.Indisponivel("Resposta vazia do PropostaService."));
            }

            var estaAprovada = string.Equals(externa.Status, "Aprovada", StringComparison.OrdinalIgnoreCase);
            return Result.Ok(new PropostaConsultada(propostaId, estaAprovada, externa.ValorPremio));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // cancelamento real do chamador — não mascarar como indisponibilidade
        }
        catch (HttpRequestException)
        {
            return Indisponivel();
        }
        catch (ExecutionRejectedException)
        {
            // Circuito aberto ou timeout do pipeline de resiliência (Polly).
            return Indisponivel();
        }
        catch (OperationCanceledException)
        {
            // Timeout (não foi cancelamento do chamador).
            return Indisponivel();
        }
    }

    private static Result<PropostaConsultada> Indisponivel()
        => Result.Falhar<PropostaConsultada>(Erro.Indisponivel("PropostaService indisponível."));

    /// <summary>Contrato externo (somente os campos consumidos). Faz parte da ACL.</summary>
    private sealed record PropostaExterna(string Status, decimal ValorPremio);
}

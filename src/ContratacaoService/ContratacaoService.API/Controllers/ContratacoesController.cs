using ContratacaoService.API.Infrastructure;
using ContratacaoService.Application.Contratacoes.Dtos;
using ContratacaoService.Application.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;

namespace ContratacaoService.API.Controllers;

[ApiController]
[Route("api/v1/contratacoes")]
[Produces("application/json")]
public sealed class ContratacoesController : ControllerBase
{
    private readonly IContratarProposta _contratar;
    private readonly IObterContratacao _obter;

    public ContratacoesController(IContratarProposta contratar, IObterContratacao obter)
    {
        _contratar = contratar;
        _obter = obter;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContratacaoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Contratar([FromBody] ContratarPropostaRequest request, CancellationToken ct)
    {
        var resultado = await _contratar.ExecutarAsync(request, ct);

        return resultado.Sucesso
            ? CreatedAtAction(nameof(Obter), new { id = resultado.Valor.Id }, resultado.Valor)
            : this.ParaProblema(resultado.Erro);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContratacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var resultado = await _obter.ExecutarAsync(id, ct);

        return resultado.Sucesso
            ? Ok(resultado.Valor)
            : this.ParaProblema(resultado.Erro);
    }
}

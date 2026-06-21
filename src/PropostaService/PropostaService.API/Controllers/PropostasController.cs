using Microsoft.AspNetCore.Mvc;
using PropostaService.API.Infrastructure;
using PropostaService.Application.Common;
using PropostaService.Application.Ports.Inbound;
using PropostaService.Application.Propostas.Dtos;

namespace PropostaService.API.Controllers;

[ApiController]
[Route("api/v1/propostas")]
[Produces("application/json")]
public sealed class PropostasController : ControllerBase
{
    private readonly ICriarProposta _criar;
    private readonly IObterProposta _obter;
    private readonly IListarPropostas _listar;
    private readonly IAlterarStatusProposta _alterarStatus;

    public PropostasController(
        ICriarProposta criar,
        IObterProposta obter,
        IListarPropostas listar,
        IAlterarStatusProposta alterarStatus)
    {
        _criar = criar;
        _obter = obter;
        _listar = listar;
        _alterarStatus = alterarStatus;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PropostaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarPropostaRequest request, CancellationToken ct)
    {
        var resultado = await _criar.ExecutarAsync(request, ct);

        return resultado.Sucesso
            ? CreatedAtAction(nameof(Obter), new { id = resultado.Valor.Id }, resultado.Valor)
            : this.ParaProblema(resultado.Erro);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PropostaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var resultado = await _obter.ExecutarAsync(id, ct);

        return resultado.Sucesso
            ? Ok(resultado.Valor)
            : this.ParaProblema(resultado.Erro);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<PropostaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? status,
        [FromQuery] string? documento,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filtro = new ListarPropostasFiltro
        {
            Status = status,
            Documento = documento,
            Pagina = page,
            TamanhoPagina = pageSize
        };

        var resultado = await _listar.ExecutarAsync(filtro, ct);

        return resultado.Sucesso
            ? Ok(resultado.Valor)
            : this.ParaProblema(resultado.Erro);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] AlterarStatusRequest request, CancellationToken ct)
    {
        var resultado = await _alterarStatus.ExecutarAsync(id, request, ct);

        return resultado.Sucesso
            ? NoContent()
            : this.ParaProblema(resultado.Erro);
    }
}

using Microsoft.AspNetCore.Mvc;
using PropostaService.Application.Common;

namespace PropostaService.API.Infrastructure;

/// <summary>Traduz um <see cref="Erro"/> de aplicação em ProblemDetails HTTP.</summary>
internal static class ApiResults
{
    public static IActionResult ParaProblema(this ControllerBase controller, Erro erro)
    {
        var status = erro.Tipo switch
        {
            TipoErro.Validacao => StatusCodes.Status400BadRequest,
            TipoErro.NaoEncontrado => StatusCodes.Status404NotFound,
            TipoErro.Conflito => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return controller.Problem(detail: erro.Mensagem, statusCode: status);
    }
}

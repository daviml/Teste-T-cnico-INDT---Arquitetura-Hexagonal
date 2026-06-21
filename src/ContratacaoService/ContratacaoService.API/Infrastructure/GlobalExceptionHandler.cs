using ContratacaoService.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.API.Infrastructure;

/// <summary>
/// Rede de segurança para exceções não tratadas → ProblemDetails. Mapeia a
/// violação de unicidade (race entre o fast-fail e o INSERT) para 409: a
/// verdade final da invariante anti-duplicação é o índice único do banco.
/// </summary>
internal sealed partial class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _ambiente;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment ambiente)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
        _ambiente = ambiente;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, titulo) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflito de persistência"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            LogErroNaoTratado(exception);
        }

        httpContext.Response.StatusCode = status;

        var detalhe = status == StatusCodes.Status500InternalServerError && !_ambiente.IsDevelopment()
            ? "Ocorreu um erro inesperado."
            : exception.Message;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = status,
                Title = titulo,
                Detail = detalhe
            }
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Erro não tratado")]
    private partial void LogErroNaoTratado(Exception exception);
}

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PropostaService.Domain.Common;

namespace PropostaService.API.Infrastructure;

/// <summary>
/// Rede de segurança para exceções NÃO tratadas. O fluxo normal de regras de
/// negócio usa Result (traduzido nos controllers); aqui caem apenas violações
/// de invariante que escaparam e erros inesperados — sempre como ProblemDetails.
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
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflito de concorrência"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            LogErroNaoTratado(exception);
        }

        httpContext.Response.StatusCode = status;

        // Não vazar detalhes internos de 500 fora de desenvolvimento.
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

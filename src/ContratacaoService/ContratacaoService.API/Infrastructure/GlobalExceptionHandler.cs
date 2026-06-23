using ContratacaoService.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        var (status, titulo) = ClassificarExcecao(exception);

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

    /// <summary>
    /// Traduz a exceção em (status HTTP, título). Só a violação de unicidade do
    /// Postgres (<c>SqlState 23505</c>, a corrida que o índice único protege) vira
    /// 409; qualquer outra <see cref="DbUpdateException"/> (FK, NOT NULL, timeout)
    /// é falha inesperada e cai em 500 — não mascaramos erro de persistência como
    /// conflito de negócio.
    /// </summary>
    internal static (int Status, string Titulo) ClassificarExcecao(Exception exception) => exception switch
    {
        DomainException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } }
            => (StatusCodes.Status409Conflict, "Conflito de persistência"),
        _ => (StatusCodes.Status500InternalServerError, "Erro interno")
    };

    [LoggerMessage(Level = LogLevel.Error, Message = "Erro não tratado")]
    private partial void LogErroNaoTratado(Exception exception);
}

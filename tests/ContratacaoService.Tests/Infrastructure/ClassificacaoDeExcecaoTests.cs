using ContratacaoService.API.Infrastructure;
using ContratacaoService.Domain.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ContratacaoService.Tests.Infrastructure;

/// <summary>
/// Garante que o handler global só traduz a violação de unicidade do Postgres
/// (23505) para 409 — qualquer outra falha de persistência deve ser 500, e não
/// um "conflito de negócio" enganoso.
/// </summary>
public class ClassificacaoDeExcecaoTests
{
    private static PostgresException ComSqlState(string sqlState) =>
        new("erro", "ERROR", "ERROR", sqlState);

    [Fact]
    public void DomainException_DeveSer400()
    {
        var (status, _) = GlobalExceptionHandler.ClassificarExcecao(new DomainException("inválido"));

        status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void UniqueViolation_DeveSer409()
    {
        var excecao = new DbUpdateException("falha", ComSqlState(PostgresErrorCodes.UniqueViolation));

        var (status, _) = GlobalExceptionHandler.ClassificarExcecao(excecao);

        status.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void ForeignKeyViolation_DeveSer500()
    {
        var excecao = new DbUpdateException("falha", ComSqlState(PostgresErrorCodes.ForeignKeyViolation));

        var (status, _) = GlobalExceptionHandler.ClassificarExcecao(excecao);

        status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void DbUpdateException_SemInnerPostgres_DeveSer500()
    {
        var (status, _) = GlobalExceptionHandler.ClassificarExcecao(new DbUpdateException("falha"));

        status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void ExcecaoGenerica_DeveSer500()
    {
        var (status, _) = GlobalExceptionHandler.ClassificarExcecao(new InvalidOperationException());

        status.Should().Be(StatusCodes.Status500InternalServerError);
    }
}

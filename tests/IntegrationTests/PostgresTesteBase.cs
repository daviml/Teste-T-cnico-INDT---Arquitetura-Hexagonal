using Testcontainers.PostgreSql;

namespace IntegrationTests;

/// <summary>
/// Base de testes de integração contra um PostgreSQL REAL (Testcontainers),
/// mesma imagem do docker-compose. Cada classe de teste sobe seu próprio
/// contêiner (isolamento) e aplica as migrations reais no <c>InitializeAsync</c>.
/// </summary>
public abstract class PostgresTesteBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    protected string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await AoIniciarAsync();
    }

    public async Task DisposeAsync()
    {
        await AoDescartarAsync();
        await _container.DisposeAsync();
    }

    /// <summary>Gancho pós-start do contêiner (ex.: aplicar migrations, montar DI).</summary>
    protected virtual Task AoIniciarAsync() => Task.CompletedTask;

    /// <summary>Gancho pré-dispose do contêiner (ex.: liberar o ServiceProvider).</summary>
    protected virtual Task AoDescartarAsync() => Task.CompletedTask;
}

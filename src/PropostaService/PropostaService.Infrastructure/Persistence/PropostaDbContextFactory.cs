using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PropostaService.Infrastructure.Persistence;

/// <summary>
/// Fabrica o DbContext em tempo de design (ex.: <c>dotnet ef migrations</c>),
/// sem depender da API. A connection string vem de variável de ambiente, com
/// um padrão para o ambiente local do docker-compose.
/// </summary>
public sealed class PropostaDbContextFactory : IDesignTimeDbContextFactory<PropostaDbContext>
{
    public PropostaDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("PROPOSTA_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=propostas_db;Username=proposta_app;Password=proposta_dev_pwd";

        var options = new DbContextOptionsBuilder<PropostaDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PropostaDbContext(options);
    }
}

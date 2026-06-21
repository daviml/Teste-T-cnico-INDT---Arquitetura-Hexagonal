using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContratacaoService.Infrastructure.Persistence;

/// <summary>
/// Fabrica o DbContext em tempo de design (dotnet ef migrations), sem depender
/// da API. Connection string via variável de ambiente, com padrão local.
/// </summary>
public sealed class ContratacaoDbContextFactory : IDesignTimeDbContextFactory<ContratacaoDbContext>
{
    public ContratacaoDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("CONTRATACAO_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=contratacoes_db;Username=contratacao_app;Password=contratacao_dev_pwd";

        var options = new DbContextOptionsBuilder<ContratacaoDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ContratacaoDbContext(options);
    }
}

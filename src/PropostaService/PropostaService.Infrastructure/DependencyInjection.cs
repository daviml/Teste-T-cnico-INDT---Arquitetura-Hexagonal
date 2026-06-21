using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Application.Ports.Outbound;
using PropostaService.Infrastructure.Persistence;

namespace PropostaService.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra a infraestrutura de persistência do PropostaService: DbContext
    /// (PostgreSQL) e as implementações dos ports de saída.
    /// </summary>
    public static IServiceCollection AddPropostaInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PropostaDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IPropostaRepository, PropostaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGeradorNumeroProposta, GeradorNumeroProposta>();

        return services;
    }
}

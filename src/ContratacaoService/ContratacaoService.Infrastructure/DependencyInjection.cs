using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Infrastructure.Gateways;
using ContratacaoService.Infrastructure.Messaging;
using ContratacaoService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContratacaoService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddContratacaoInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Uri propostaServiceBaseUrl)
    {
        services.AddDbContext<ContratacaoDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IContratacaoRepository, ContratacaoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Placeholder de mensageria (substituído pelo Outbox no Commit 12).
        services.AddScoped<IEventBus, LoggingEventBus>();

        // Gateway HTTP resiliente: timeout + retry + circuit breaker (Polly v8).
        services
            .AddHttpClient<IPropostaGateway, PropostaGateway>(client => client.BaseAddress = propostaServiceBaseUrl)
            .AddStandardResilienceHandler();

        return services;
    }
}

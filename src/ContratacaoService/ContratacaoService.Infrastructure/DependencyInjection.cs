using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Infrastructure.Gateways;
using ContratacaoService.Infrastructure.Messaging;
using ContratacaoService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContratacaoService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddContratacaoInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Uri propostaServiceBaseUrl,
        RabbitMqOptions rabbitMq)
    {
        services.AddDbContext<ContratacaoDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IContratacaoRepository, ContratacaoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Mensageria via Transactional Outbox: o adapter traduz o domain event e
        // publica; o Bus Outbox grava na transação do DbContext e entrega depois.
        services.AddScoped<IEventBus, MassTransitEventBus>();
        services.AddContratacaoMessaging(rabbitMq);

        // Gateway HTTP resiliente: timeout + retry + circuit breaker (Polly v8).
        services
            .AddHttpClient<IPropostaGateway, PropostaGateway>(client => client.BaseAddress = propostaServiceBaseUrl)
            .AddStandardResilienceHandler();

        return services;
    }

    private static IServiceCollection AddContratacaoMessaging(this IServiceCollection services, RabbitMqOptions rabbitMq)
    {
        services.AddMassTransit(bus =>
        {
            // Outbox transacional sobre o DbContext do serviço (Postgres).
            bus.AddEntityFrameworkOutbox<ContratacaoDbContext>(outbox =>
            {
                outbox.UsePostgres();

                // O IPublishEndpoint do escopo passa a escrever no Outbox em vez
                // de ir direto ao broker — atomicidade com o SaveChanges.
                outbox.UseBusOutbox();
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMq.Host, host =>
                {
                    host.Username(rabbitMq.Username);
                    host.Password(rabbitMq.Password);
                });

                // Sem receive endpoints: este serviço é publisher puro (não consome).
            });
        });

        return services;
    }
}

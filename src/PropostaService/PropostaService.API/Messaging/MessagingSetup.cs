using MassTransit;
using PropostaService.Infrastructure.Persistence;

namespace PropostaService.API.Messaging;

public static class MessagingSetup
{
    /// <summary>
    /// Configura a mensageria do PropostaService como CONSUMIDOR: registra o
    /// consumer de <c>ContratacaoRealizada</c>, o Inbox transacional (dedupe de
    /// reentrega sobre o <see cref="PropostaDbContext"/>) e o transporte RabbitMQ.
    /// Sem <c>UseBusOutbox</c>: este serviço só consome, não publica eventos.
    /// </summary>
    public static IServiceCollection AddPropostaMessaging(this IServiceCollection services, RabbitMqOptions rabbitMq)
    {
        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<ContratacaoRealizadaConsumer>();

            bus.AddEntityFrameworkOutbox<PropostaDbContext>(outbox => outbox.UsePostgres());

            // Aplica o Inbox (dedupe por MessageId, na transação do DbContext) a
            // todos os receive endpoints. Sem este callback o ConfigureEndpoints
            // não ativa o Inbox e reentregas seriam reprocessadas.
            bus.AddConfigureEndpointsCallback((context, _, cfg) =>
                cfg.UseEntityFrameworkOutbox<PropostaDbContext>(context));

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMq.Host, host =>
                {
                    host.Username(rabbitMq.Username);
                    host.Password(rabbitMq.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

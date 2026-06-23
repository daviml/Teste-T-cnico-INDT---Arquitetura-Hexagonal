using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PropostaService.API.Health;
using PropostaService.API.Infrastructure;
using PropostaService.API.Messaging;
using PropostaService.API.Observability;
using PropostaService.Application.Ports.Inbound;
using PropostaService.Application.Propostas.UseCases;
using PropostaService.Infrastructure;
using System.Globalization;
using Serilog;
using Serilog.Events;

// Logger de bootstrap: captura falhas já na inicialização (ex.: migrations).
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog como provider do Microsoft.Extensions.Logging (substitui os padrões).
    // Enriquecido com o CorrelationId do LogContext (ver CorrelationIdMiddleware).
    builder.Host.UseSerilog((context, cfg) => cfg
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            formatProvider: CultureInfo.InvariantCulture,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

    // --- Driving adapters: controllers + erros padronizados (ProblemDetails) ---
    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
            // Enums trafegam como texto (ex.: status "Aprovada").
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // OpenAPI (documento consumido pela Swagger UI em desenvolvimento).
    builder.Services.AddOpenApi();

    // --- Composição: infraestrutura + casos de uso ---
    var connectionString = builder.Configuration.GetConnectionString("Propostas")
        ?? throw new InvalidOperationException("Connection string 'Propostas' não configurada.");

    builder.Services.AddPropostaInfrastructure(connectionString);

    // --- Mensageria: consumer idempotente (Inbox) de ContratacaoRealizada ---
    var rabbitMq = new RabbitMqOptions(
        builder.Configuration["RabbitMq:Host"] ?? throw new InvalidOperationException("'RabbitMq:Host' não configurado."),
        builder.Configuration["RabbitMq:Username"] ?? throw new InvalidOperationException("'RabbitMq:Username' não configurado."),
        builder.Configuration["RabbitMq:Password"] ?? throw new InvalidOperationException("'RabbitMq:Password' não configurado."));

    builder.Services.AddPropostaMessaging(rabbitMq);

    builder.Services.AddScoped<ICriarProposta, CriarPropostaHandler>();
    builder.Services.AddScoped<IObterProposta, ObterPropostaHandler>();
    builder.Services.AddScoped<IListarPropostas, ListarPropostasHandler>();
    builder.Services.AddScoped<IAlterarStatusProposta, AlterarStatusPropostaHandler>();
    builder.Services.AddScoped<IMarcarComoContratada, MarcarComoContratadaHandler>();

    // --- Health checks ---
    builder.Services.AddHealthChecks()
        .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

    var app = builder.Build();

    // Correlation id primeiro: todo log/erro subsequente já o carrega.
    app.UseCorrelationId();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "PropostaService v1"));
    }

    app.UseHttpsRedirection();
    app.MapControllers();

    // Liveness: o processo responde. Readiness: dependências (banco) OK.
    app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
    app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = registro => registro.Tags.Contains("ready") });

    await app.AplicarMigracoesAsync();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "PropostaService encerrado inesperadamente na inicialização.");
}
finally
{
    Log.CloseAndFlush();
}

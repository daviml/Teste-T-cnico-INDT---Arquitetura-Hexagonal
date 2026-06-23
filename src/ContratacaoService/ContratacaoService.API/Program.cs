using ContratacaoService.API.Health;
using ContratacaoService.API.Infrastructure;
using ContratacaoService.Application.Contratacoes.UseCases;
using ContratacaoService.Application.Ports.Inbound;
using ContratacaoService.Infrastructure;
using ContratacaoService.Infrastructure.Messaging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// --- Driving adapters: controllers + erros padronizados (ProblemDetails) ---
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

// --- Composição: infraestrutura (banco + gateway resiliente) + casos de uso ---
var connectionString = builder.Configuration.GetConnectionString("Contratacoes")
    ?? throw new InvalidOperationException("Connection string 'Contratacoes' não configurada.");

var propostaBaseUrl = builder.Configuration["PropostaService:BaseUrl"]
    ?? throw new InvalidOperationException("'PropostaService:BaseUrl' não configurada.");

var rabbitMq = new RabbitMqOptions(
    builder.Configuration["RabbitMq:Host"] ?? throw new InvalidOperationException("'RabbitMq:Host' não configurado."),
    builder.Configuration["RabbitMq:Username"] ?? throw new InvalidOperationException("'RabbitMq:Username' não configurado."),
    builder.Configuration["RabbitMq:Password"] ?? throw new InvalidOperationException("'RabbitMq:Password' não configurado."));

builder.Services.AddContratacaoInfrastructure(connectionString, new Uri(propostaBaseUrl), rabbitMq);

builder.Services.AddScoped<IContratarProposta, ContratarPropostaHandler>();
builder.Services.AddScoped<IObterContratacao, ObterContratacaoHandler>();

// --- Health checks ---
builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "ContratacaoService v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = registro => registro.Tags.Contains("ready") });

await app.AplicarMigracoesAsync();

await app.RunAsync();

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PropostaService.API.Health;
using PropostaService.API.Infrastructure;
using PropostaService.Application.Ports.Inbound;
using PropostaService.Application.Propostas.UseCases;
using PropostaService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<ICriarProposta, CriarPropostaHandler>();
builder.Services.AddScoped<IObterProposta, ObterPropostaHandler>();
builder.Services.AddScoped<IListarPropostas, ListarPropostasHandler>();
builder.Services.AddScoped<IAlterarStatusProposta, AlterarStatusPropostaHandler>();
builder.Services.AddScoped<IMarcarComoContratada, MarcarComoContratadaHandler>();

// --- Health checks ---
builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

var app = builder.Build();

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

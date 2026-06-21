using Microsoft.EntityFrameworkCore;
using PropostaService.Infrastructure.Persistence;

namespace PropostaService.API.Infrastructure;

internal static class MigrationExtensions
{
    /// <summary>
    /// Aplica migrations pendentes no startup. Conveniência para o ambiente do
    /// desafio; o MigrateAsync do EF adquire um lock que evita corrida entre
    /// réplicas. Em produção, o ideal é um passo de deploy dedicado.
    /// </summary>
    public static async Task AplicarMigracoesAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
        await context.Database.MigrateAsync();
    }
}

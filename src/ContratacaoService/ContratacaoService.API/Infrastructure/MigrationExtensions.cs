using ContratacaoService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.API.Infrastructure;

internal static class MigrationExtensions
{
    /// <summary>
    /// Aplica migrations pendentes no startup (conveniência do desafio; o
    /// MigrateAsync adquire lock contra corrida entre réplicas). Em produção,
    /// o ideal é um passo de deploy dedicado.
    /// </summary>
    public static async Task AplicarMigracoesAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();
        await context.Database.MigrateAsync();
    }
}

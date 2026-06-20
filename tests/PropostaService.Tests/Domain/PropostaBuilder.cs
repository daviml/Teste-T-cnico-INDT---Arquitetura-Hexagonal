using PropostaService.Domain.Propostas;
using PropostaService.Domain.Propostas.ValueObjects;

namespace PropostaService.Tests.Domain;

/// <summary>
/// Constrói uma <see cref="Proposta"/> válida para os testes, permitindo variar
/// só o que importa em cada cenário. Os eventos de criação são limpos por padrão
/// para que cada teste verifique apenas os eventos da transição que exercita.
/// </summary>
internal static class PropostaBuilder
{
    public static Proposta Valida(decimal cobertura = 1000m)
    {
        var proposta = Proposta.Criar(
            NumeroProposta.Gerar(2026, 1),
            "Cliente Teste",
            Documento.Criar("52998224725"),
            Email.Criar("cliente@teste.com"),
            Dinheiro.De(cobertura));

        proposta.LimparEventos();
        return proposta;
    }
}

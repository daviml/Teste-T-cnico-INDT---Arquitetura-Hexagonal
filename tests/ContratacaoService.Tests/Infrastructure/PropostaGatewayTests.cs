using System.Net;
using ContratacaoService.Application.Common;
using ContratacaoService.Infrastructure.Gateways;
using FluentAssertions;

namespace ContratacaoService.Tests.Infrastructure;

public class PropostaGatewayTests
{
    private static PropostaGateway GatewayCom(HttpStatusCode status, string? json = null)
    {
        var handler = new RespostaFalsaHandler(status, json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://proposta.local") };
        return new PropostaGateway(httpClient);
    }

    [Fact]
    public async Task ObterProposta_Com200EAprovada_DeveTraduzirParaAprovada()
    {
        var gateway = GatewayCom(HttpStatusCode.OK, """{"status":"Aprovada","valorPremio":500.00}""");

        var resultado = await gateway.ObterPropostaAsync(Guid.NewGuid());

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.EstaAprovada.Should().BeTrue();
        resultado.Valor.ValorPremio.Should().Be(500.00m);
    }

    [Fact]
    public async Task ObterProposta_Com200ENaoAprovada_DeveTraduzirParaNaoAprovada()
    {
        var gateway = GatewayCom(HttpStatusCode.OK, """{"status":"EmAnalise","valorPremio":500.00}""");

        var resultado = await gateway.ObterPropostaAsync(Guid.NewGuid());

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.EstaAprovada.Should().BeFalse();
    }

    [Fact]
    public async Task ObterProposta_Com404_DeveRetornarNaoEncontrado()
    {
        var gateway = GatewayCom(HttpStatusCode.NotFound);

        var resultado = await gateway.ObterPropostaAsync(Guid.NewGuid());

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.NaoEncontrado);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task ObterProposta_ComErro5xx_DeveRetornarIndisponivel(HttpStatusCode status)
    {
        var gateway = GatewayCom(status);

        var resultado = await gateway.ObterPropostaAsync(Guid.NewGuid());

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Tipo.Should().Be(TipoErro.Indisponivel);
    }

    private sealed class RespostaFalsaHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string? _json;

        public RespostaFalsaHandler(HttpStatusCode status, string? json)
        {
            _status = status;
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resposta = new HttpResponseMessage(_status);
            if (_json is not null)
            {
                resposta.Content = new StringContent(_json, System.Text.Encoding.UTF8, "application/json");
            }

            return Task.FromResult(resposta);
        }
    }
}

namespace ContratacaoService.Infrastructure.Messaging;

/// <summary>
/// Dados de conexão do RabbitMQ. Lidos da configuração na API e repassados à
/// composição da infraestrutura (mantém a Infrastructure livre de IConfiguration).
/// </summary>
public sealed record RabbitMqOptions(string Host, string Username, string Password);

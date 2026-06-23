namespace PropostaService.API.Messaging;

/// <summary>
/// Dados de conexão do RabbitMQ. Lidos da configuração e repassados à composição
/// da mensageria (mantém o setup explícito, sem espalhar IConfiguration).
/// </summary>
public sealed record RabbitMqOptions(string Host, string Username, string Password);

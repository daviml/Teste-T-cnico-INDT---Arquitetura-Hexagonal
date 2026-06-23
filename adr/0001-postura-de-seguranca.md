# ADR 0001 — Postura de segurança e escopo

- **Status:** aceito
- **Data:** 2026-06-23
- **Contexto:** teste técnico (arquitetura hexagonal/DDD). O critério é *julgamento de arquitetura*, não acúmulo de ferramentas.

## Contexto

Dois microserviços (`PropostaService`, `ContratacaoService`) expõem APIs REST e se
integram de forma síncrona (HTTP) e assíncrona (RabbitMQ). É preciso definir, de
forma explícita, **o que está dentro** da postura de segurança desta entrega e
**o que fica conscientemente fora** — para que a ausência de um controle seja lida
como decisão, não como esquecimento.

## Decisão

### O que existe nesta entrega

1. **Rate limiting nativo na borda** (`Microsoft.AspNetCore.RateLimiting`):
   janela fixa por cliente (IP) nos **endpoints de escrita** (POST/PATCH). O
   excesso responde **`429 Too Many Requests`** em `ProblemDetails`, com
   `Retry-After`. É o par **inbound** da resiliência **outbound** (Polly no
   gateway HTTP): proteção de sobrecarga nas duas direções.
   - Implementado como **concern transversal na borda da API** (driving adapter),
     sem tocar domínio nem Application — coerente com a arquitetura hexagonal.
2. **Isolamento de dados por serviço**: bancos e usuários PostgreSQL separados
   (`propostas_db`/`proposta_app`, `contratacoes_db`/`contratacao_app`), cada
   serviço só enxerga o seu schema.
3. **Segredos via variáveis de ambiente** (`.env` git-ignored; `.env.example`
   versionado com placeholders de desenvolvimento).
4. **`ProblemDetails` que não vaza detalhes internos**: em produção, o handler
   global de exceções devolve mensagem genérica para `500` (o `Detail` real só
   aparece em Development).

### O que fica conscientemente FORA (e por quê)

- **Autenticação/autorização** (JWT, OAuth2, API keys).
- **Gestão de segredos em vault** (ex.: Azure Key Vault, HashiCorp Vault).
- **Rate limiting centralizado/global** entre instâncias.

Em produção, esses controles seriam responsabilidade de um **API Gateway /
ingress** à frente dos serviços (terminação de TLS, authN/Z, *rate limiting*
distribuído, WAF), e **não** de cada microserviço. Implementá-los aqui, em cada
serviço, seria **over-engineering** para o escopo do teste e contradiria o
princípio de manter o desenho simples (YAGNI consciente).

## Consequências

- **Positivas:** a superfície de escrita fica protegida contra abuso trivial; a
  postura é explícita e auditável; o domínio/Application permanecem livres de
  preocupações de borda; a evolução para um API Gateway é natural (o rate limiting
  local é facilmente substituível/complementável).
- **Negativas / limitações:** sem authN/Z, os endpoints são abertos no ambiente
  do teste; o rate limiting por instância não coordena entre réplicas (aceitável
  para demonstração — o gateway resolveria em produção).

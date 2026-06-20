# Teste Técnico INDT — Arquitetura Hexagonal

Sistema de **propostas e contratações de seguro** composto por dois microserviços independentes,
construído com **Arquitetura Hexagonal (Ports & Adapters)** e **DDD**, com isolamento físico de
dados por serviço e comunicação síncrona (HTTP REST) + assíncrona (RabbitMQ).

> 🚧 **Projeto em construção.** Este README é um placeholder inicial. As instruções completas de
> build, execução, exemplos de chamada e decisões de arquitetura serão preenchidas ao final da
> implementação (ver roteiro no [plano de implementação](./plano%20de%20implementação.md)).

---

## Stack

- **.NET / C#** — microserviços (`PropostaService` e `ContratacaoService`)
- **Arquitetura Hexagonal** — 4 projetos por serviço (Domain · Application · Infrastructure · API)
- **PostgreSQL** — um servidor, dois bancos lógicos com usuários distintos por serviço
- **RabbitMQ + MassTransit** — mensageria assíncrona (Transactional Outbox / Inbox)
- **EF Core** — persistência e migrations
- **Polly** — resiliência na comunicação HTTP entre serviços
- **Serilog + OpenTelemetry** — observabilidade
- **xUnit + Testcontainers** — testes unitários e de integração
- **Docker Compose** — orquestração do ambiente local

## Arquitetura

Dois microserviços autônomos, com a regra de dependência apontando sempre para o domínio:

```
API (Driving Adapters)  →  Application (Use Cases + Ports)  →  Domain
Infrastructure (Driven Adapters)  ──implementa Ports──↑
```

- **PropostaService** — criação, listagem e mudança de status de propostas de seguro.
- **ContratacaoService** — efetivação de contratações a partir de propostas aprovadas.

Diagrama detalhado, fluxo ponta a ponta e justificativas de design no
[plano de implementação](./plano%20de%20implementação.md).

---

## Como executar

🚧 *Em construção.* Será documentado ao final (essencialmente `docker-compose up --build`,
com portas, URLs do Swagger e exemplos de chamada).

## Testes

🚧 *Em construção.* (`dotnet test` — unitários e integração com Testcontainers.)

## Decisões de Arquitetura (ADRs)

🚧 *Em construção.* Trade-offs e decisões conscientes (incluindo o que **não** foi feito por YAGNI)
serão consolidados aqui. Racional preliminar documentado no
[plano de implementação](./plano%20de%20implementação.md).

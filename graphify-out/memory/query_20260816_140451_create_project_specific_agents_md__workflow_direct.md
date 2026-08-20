---
type: "query"
date: "2026-08-16T14:04:51.192432+00:00"
question: "Create project-specific AGENTS.md, workflow directives, and skills from the supplied generic directives, including security."
contributor: "graphify"
outcome: "useful"
source_nodes: ["Program", "ApplicationDbContext", "SteamApp.Tests", "AuthService", "openableExternalUrl", "RabbitMqOptions"]
---

# Q: Create project-specific AGENTS.md, workflow directives, and skills from the supplied generic directives, including security.

## Answer

Expanded from original query via graph vocab: [angular, server, controller, service, repository, context, authentication, authorization, rabbitmq, redis, scraping, tests]. SteamApp uses an Angular 21 client and a .NET 9 server split across Domain, Application, Interfaces, Infrastructure, WebAPI, WebApiClient, and three NUnit test projects. ApplicationDbContext hosts Identity and business data; reusable/background paths use IDbContextFactory while request-bound Minimal APIs may use a scoped context. DI is composed in Program.cs and provider-specific RabbitMQ extensions. Security-sensitive paths include JWT/Identity policies, user ownership, external URL disclosure, scraping, RabbitMQ, Redis, and client token handling.

## Outcome

- Signal: useful

## Source Nodes

- Program
- ApplicationDbContext
- SteamApp.Tests
- AuthService
- openableExternalUrl
- RabbitMqOptions
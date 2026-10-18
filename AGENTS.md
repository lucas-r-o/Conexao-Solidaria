# Conexão Solidaria

## Project
ConexaoSolidaria: .NET microservices donation platform.
Solution: `src/ConexaoSolidaria.slnx`

## Infrastruture
- Database: Supabase (PostgreSQL)
- Messaging: RabbitMQ, started with Docker Compose

## Related services
- The Worker is a separate microservice in another repository. It is not part of this solution.
- This API publishes events to RabbitMQ (via MassTransit) and the Worker consumes them.
- Do not change event contracts (message classes) without checking that the Worker stays compatible.

## Running locally
- `docker-compose.yml` starts only RabbitMQ. It does not run the application.
- Start RabbitMQ first: `docker compose -f src/docker-compose.yml up -d`
- Then run the API: `dotnet run --project src/ConexaoSolidaria.API`
- Wait for RabbitMQ to be ready before starting the app.
- Stop RabbitMQ: `docker compose -f src/docker-compose.yml down`

## Commands
- Restore: `dotnet restore src/ConexaoSolidaria.slnx`
- Build: `dotnet build src/ConexaoSolidaria.slnx`
- Test all: `dotnet test src/ConexaoSolidaria.Tests`
- Run locally: `docker compose -f src/docker-compose.yml up --build`

## Code conventions
- C# with nullable reference types enabled.
- PascalCase for classes and methods, `_camelCase` for private fields.
- Interfaces start with `I` (e.g. `IDonationRepository`).
- Use async/await with `Async` suffix; pass `CancellationToken` when available.
- Use dependency injection. No `new` for services.
- Keep controllers thin: validate input, call an ApplicationService, return the result.

## Rules
- Run the tests after changing `ApplicationServices` or `Infrastructure`.
- Add or update tests for every new behavior.
- Follow existing patterns and naming before introducing new ones.
- Do not edit `.github` workflows, Dockerfiles, or migrations without asking.
- Do not commit secrets or connection strings. Use configuration or environment variables.
- Make small, focused changes. Don't refactor unrelated code.

## Commits
- Conventional Commits (feat, fix, refactor, test, docs, chore), English, imperative mood, max 72 chars in the subject.
- If the changes cover more than one concern, split them into multiple commits, one logical change per commit.
- Order commits so each one builds on its own: refactors first, then features or fixes, then tests, then docs.
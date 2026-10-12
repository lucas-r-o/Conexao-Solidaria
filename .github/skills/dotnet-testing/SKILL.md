---
name: dotnet-testing
description: Write, review, or refactor automated tests for the ConexaoSolidaria .NET solution. Covers unit tests (business rules in ApplicationServices, isolated with Moq), integration tests (service + EF Core + MassTransit wiring), and functional/API tests (end-to-end HTTP calls against the running API). Use this whenever the user asks to add a test, refactor an existing test, review test coverage, split a test into the right category, or fix a failing test in this project — even if they just say "write a test" without naming a type.
---

# .NET Testing — ConexaoSolidaria

Encodes this project's test conventions so tests are correctly categorized, isolated at the right boundary, and consistent with `AGENTS.md`.

## Step 1 — Classify before writing

Ask: **what is this test allowed to touch?**

| Touches only in-memory objects + mocks, no DbContext, no DI container | → **Unit test** — read `references/unit-tests.md` |
| Touches a real (or InMemory/Testcontainers) `DbContext`, resolves services via DI, or publishes through MassTransit's test harness | → **Integration test** — read `references/integration-tests.md` |
| Boots the API itself (`WebApplicationFactory`) and calls it over HTTP, asserting on responses/persisted state | → **Functional test** — read `references/functional-tests.md` |

If the user hands you an existing test and asks to "refactor" it, classify it first and say out loud which category it actually is — don't assume the folder name or class name is correct. A test that spins up a full `ServiceCollection` and hits an `AppDbContext` is an integration test even if it lives next to unit tests and is named `*Tests.cs`.

**Do not build functional tests with Playwright.** Playwright drives a browser against rendered UI. This solution is an API only (`src/ConexaoSolidaria.API`) — functional/E2E coverage here means HTTP calls via `WebApplicationFactory<Program>` + `HttpClient`, not a browser. Only reach for Playwright if a separate frontend consuming this API is in scope.

## Step 2 — Follow project rules (from AGENTS.md)

- Naming: `MethodName_Scenario_ExpectedResult` (e.g. `CadastrarAsync_DeveLancarArgumentException_QuandoEmailJaExistir`).
- Arrange-Act-Assert, one behavior per test.
- Nullable reference types on; `Async` suffix + `CancellationToken` on async methods being tested.
- Tests live in `src/ConexaoSolidaria.Tests`. Run with `dotnet test src/ConexaoSolidaria.Tests`.
- Run the full test suite after any change to `ApplicationServices` or `Infrastructure`.
- Add or update a test for every new behavior — don't leave new branches uncovered.
- **Never change event contracts (MassTransit message classes) to make a test pass** — the Worker (separate repo) depends on them. If a test seems to require a contract change, stop and flag it instead of editing the message class.
- Don't touch `.github` workflows, Dockerfiles, or migrations to get a test running — ask first.
- Keep changes scoped to the test/behavior at hand; don't refactor unrelated tests while you're in a file.

## Step 3 — Pick what to mock vs. what to keep real

Mock only things with real side effects or I/O. Everything else, use the real object — a mock there just adds a maintenance burden with no isolation benefit.

**Mock:**
- `I*Repository`, `IPublishEndpoint` — in unit tests (Moq). In integration tests, use the real repository/`MassTransit.Testing` harness instead (see below).
- Anything wrapping an external system this repo doesn't own: HTTP clients to third parties, `IDateTimeProvider`/clock, file system, email sender.

**Don't mock — use the real thing:**
- DTOs, entities, value objects, enums: construct them directly, never `Mock<CadastroDto>`.
- Pure domain logic (validators, calculators, mappers) with no I/O: call the real class. Mocking it just to unit-test the thing that calls it hides bugs in the logic itself.
- `AppDbContext` in **integration** tests: use the real EF Core context (InMemory/Testcontainers), not a mocked repository — that's the point of an integration test. Only mock the DbContext-equivalent in unit tests, and only via a repository interface, never mock `DbContext`/`DbSet` directly (EF Core's own APIs aren't designed to be mocked cleanly).
- Don't mock a type you don't own (framework/3rd-party classes) — wrap it in your own interface first, then mock that interface. Mocking `HttpClient` or `DbContext` directly tends to break silently when the library updates.
- If a unit test needs 4+ mocks to construct the service, that's usually a signal the service has too many responsibilities — flag it rather than adding another mock.

`AppDbContext`: never in unit tests. InMemory provider or Testcontainers-Postgres in integration tests (see `references/integration-tests.md` for which to use when).
RabbitMQ/MassTransit: never start real RabbitMQ in tests. Use `MassTransit.Testing`'s `InMemoryTestHarness` to assert an event was published — this also protects the contract-compatibility rule above, since the harness fails loudly if the message shape breaks.
The Worker itself is out of scope entirely — never attempt to spin it up or assert on its behavior from this repo.

## Step 4 — Verify

After writing or editing tests: run `dotnet test src/ConexaoSolidaria.Tests` and report the result. Don't claim a test passes without having run it.
# Functional (API) tests

**Goal:** verify a full request through the real API — routing, model binding, controller → ApplicationServices → Infrastructure — the way a real client would call it.

## Why not Playwright here
Playwright automates a browser against rendered pages. `ConexaoSolidaria.API` has no UI to render — it's an HTTP API. The .NET-native equivalent of "functional/E2E" for an API is `Microsoft.AspNetCore.Mvc.Testing`'s `WebApplicationFactory<TEntryPoint>`, which boots the app in-memory and gives you a real `HttpClient` to call it with. Reach for Playwright only if a separate frontend project (outside this repo) needs browser-level coverage.

## Rules
- One `WebApplicationFactory<Program>` per test class (implement `IClassFixture<WebApplicationFactory<Program>>`), reused across tests in that class for speed.
- Override configuration/DI in `WithWebHostBuilder` to swap the real Postgres/RabbitMQ for test doubles (InMemory EF, MassTransit test harness) — functional tests should still not hit real external infra.
- Assert on the actual HTTP contract: status code, response shape/DTO, and — where relevant — the resulting DB state via a scoped `AppDbContext` pulled from the factory's services.
- Cover the paths a real client hits: validation errors (400), not-found (404), success (200/201), not just the happy path.

## Pattern

```csharp
public class UsuarioEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsuarioEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));

                services.RemoveAll<IPublishEndpoint>();
                // register MassTransit InMemoryTestHarness here instead
            });
        }).CreateClient();
    }

    [Fact]
    public async Task PostCadastro_DeveRetornar201_QuandoDadosValidos()
    {
        var response = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            Nome = "Doador Teste",
            Email = "teste@exemplo.com",
            CPF = "11144477735",
            SenhaHash = "Senha123!"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

Adjust the route (`/api/usuarios`) and DTO shape to match the real controller — check `src/ConexaoSolidaria.API` rather than guessing.
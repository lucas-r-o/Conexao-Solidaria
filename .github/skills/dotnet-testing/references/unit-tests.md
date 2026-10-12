# Unit tests

**Goal:** verify one business rule in `ApplicationServices`, in isolation. No `DbContext`, no `ServiceCollection`/DI container, no real MassTransit.

## Rules
- Instantiate the service under test directly with `new`, passing mocked dependencies (Moq). Do not resolve it from a DI container — that's what makes a test an integration test instead.
- Mock every dependency: `Mock<IUsuarioRepository>`, `Mock<IPublishEndpoint>`, etc. Set up only the calls the test needs.
- No `IConfiguration` builder, no `DbContextOptionsBuilder`. If the service needs config values, mock/stub the interface that reads them, not the config system itself.
- One assertion focus per test — a test can have multiple `Assert` calls, but they should all check the same outcome (e.g. "the result is a failure with this specific error"), not unrelated things.
- Fast: a unit test suite should run in well under a second total. If a test needs `await Task.Delay` or real time to pass, it's not a unit test.

## Worked correction

The example the team currently has (`UsuarioServiceTests`) builds a `ServiceCollection`, registers a real `AppDbContext` (InMemory) and resolves `IUsuarioService` from the provider. That's an **integration test** — see `integration-tests.md`. Here's what the true unit-test equivalent looks like:

```csharp
using ApplicationServices.Services;
using Models.Dtos;
using Moq;

namespace ConexaoSolidaria.Tests.Unit;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<IPublishEndpoint> _publishMock;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _publishMock = new Mock<IPublishEndpoint>();
        _service = new UsuarioService(_repositoryMock.Object, _publishMock.Object);
    }

    [Fact]
    public async Task CadastrarAsync_DeveLancarArgumentException_QuandoEmailJaExistir()
    {
        var novoUsuario = new CadastroDto
        {
            Nome = "Novo",
            Email = "duplicado@exemplo.com",
            CPF = "11144477735",
            SenhaHash = "Senha123!"
        };

        _repositoryMock
            .Setup(r => r.ExisteEmailAsync(novoUsuario.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var cts = new CancellationTokenSource();
        var result = await _service.CadastrarAsync(novoUsuario, cts.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("E-mail já cadastrado.", result.Error);
        _repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
```

Notice the constructor signature (`UsuarioRepository`, `IPublishEndpoint`) is a guess based on typical layering — adapt it to the real constructor of `UsuarioService`. If the constructor currently only accepts `AppDbContext` directly (no repository interface), that's a testability gap: flag it to the user rather than silently wrapping the DbContext in a fake repository they didn't ask for.
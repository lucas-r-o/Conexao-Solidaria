using ApplicationServices.Interfaces;
using ApplicationServices.Services;
using Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.Dtos;
using Moq;

namespace ConexaoSolidaria.Tests;

public class UsuarioServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IPublishEndpoint> _publishMock;
    private readonly IUsuarioService _service;

    public UsuarioServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _publishMock = new Mock<IPublishEndpoint>();

        var configuration = new ConfigurationBuilder()
                               .AddInMemoryCollection(new Dictionary<string, string?>
                               {
                                    { "Jwt:Key", "chave-de-teste-com-pelo-menos-32-caracteres-aqui" },
                                    { "ConnectionStrings:CONEXAO_SOLIDARIA", "" },
                               })
                               .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(_context);
        services.AddSingleton(_publishMock.Object);
        services.AddServices();

        var provider = services.BuildServiceProvider();
        _service = provider.GetRequiredService<IUsuarioService>();
    }

    [Fact]
    public async Task CadastrarAsync_DeveSalvarUsuario_QuandoDadosForemValidos()
    {
        var usuario = new CadastroDto
        {
            Nome = "Doador Teste",
            Email = "teste@exemplo.com",
            CPF = "11144477735",
            SenhaHash = "Senha123!"
        };

        using var cts = new CancellationTokenSource();
        var result = await _service.CadastrarAsync(usuario, cts.Token);

        Assert.NotNull(result.Value);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CadastrarAsync_DeveLancarArgumentException_QuandoEmailJaExistir()
    {
        using var cts = new CancellationTokenSource();

        var usuarioExistente = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Existente",
            Email = "duplicado@exemplo.com",
            CPF = "98765432100",
            SenhaHash = "hash"
        };
        _context.Usuarios.Add(usuarioExistente);
        await _context.SaveChangesAsync(cts.Token);

        var novoUsuario = new CadastroDto
        {
            Nome = "Novo",
            Email = "duplicado@exemplo.com",
            CPF = "11144477735",
            SenhaHash = "Senha123!"
        };

        var result = await _service.CadastrarAsync(novoUsuario, cts.Token);
        Assert.Equal("E-mail já cadastrado.", result.Error);
    }
}

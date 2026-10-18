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

public class UserServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IPublishEndpoint> _publishMock;
    private readonly IUserService _service;

    public UserServiceTests()
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
        _service = provider.GetRequiredService<IUserService>();
    }

    [Fact]
    public async Task CadastrarAsync_DeveSalvarUsuario_QuandoDadosForemValidos()
    {
        var user = new RegistrationDto
        {
            Name = "Donor Teste",
            Email = "teste@exemplo.com",
            TaxId = "11144477735",
            PasswordHash = "Senha123!"
        };

        using var cts = new CancellationTokenSource();
        var result = await _service.RegisterAsync(user, cts.Token);

        Assert.NotNull(result.Value);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CadastrarAsync_DeveLancarArgumentException_QuandoEmailJaExistir()
    {
        using var cts = new CancellationTokenSource();

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Existente",
            Email = "duplicado@exemplo.com",
            TaxId = "98765432100",
            PasswordHash = "hash"
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync(cts.Token);

        var newUser = new RegistrationDto
        {
            Name = "Novo",
            Email = "duplicado@exemplo.com",
            TaxId = "11144477735",
            PasswordHash = "Senha123!"
        };

        var result = await _service.RegisterAsync(newUser, cts.Token);
        Assert.Equal("E-mail já cadastrado.", result.Error);
    }
}

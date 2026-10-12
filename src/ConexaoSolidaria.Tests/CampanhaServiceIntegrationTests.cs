using ApplicationServices.Interfaces;
using ApplicationServices.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.Dtos;
using Models.DTOs;
using Xunit;

namespace ConexaoSolidaria.Tests;

public class CampanhaServiceIntegrationTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly AppDbContext _context;
    private readonly ICampanhaService _service;

    public CampanhaServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(serviceOptions => serviceOptions.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddServices();

        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _service = _scope.ServiceProvider.GetRequiredService<ICampanhaService>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    [Fact]
    public async Task CriarAsync_DeveCriarCampanha_ComDadosValidos()
    {
        // Arrange
        var dto = new CriarCampanhaDTO
        {
            Titulo = "Minha Campanha",
            Descricao = "Descrição da campanha",
            MetaFinanceira = 1000.00m,
            DataFim = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await _service.CriarAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Minha Campanha", result.Titulo);
        Assert.Equal("Descrição da campanha", result.Descricao);
        Assert.Equal(1000.00m, result.MetaFinanceira);
        Assert.Equal(Status.Ativo, result.Status);
        Assert.Equal(0, result.ValorArrecadado);
        Assert.NotEqual(default, result.DataInicio);
        Assert.NotNull(result.DataFim);
    }

    [Fact]
    public async Task CriarAsync_DeveIncluirCampanhaNoBanco()
    {
        // Arrange
        var dto = new CriarCampanhaDTO
        {
            Titulo = "Campanha para Teste",
            Descricao = "Teste de inserção",
            MetaFinanceira = 500.00m,
            DataFim = DateTime.UtcNow.AddDays(15)
        };

        // Act
        await _service.CriarAsync(dto);
        var campanhas = await _context.Campanhas.ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(campanhas);
        Assert.Equal("Campanha para Teste", campanhas[0].Titulo);
    }

    [Fact]
    public async Task ObterCampanhasAsync_DeveRetornarCampanhasAtivas()
    {
        // Arrange
        var campanhaAtiva = new Campanha
        {
            Id = Guid.NewGuid(),
            Titulo = "Campanha Ativa",
            Descricao = "Descrição",
            MetaFinanceira = 1000.00m,
            Status = Status.Ativo,
            ValorArrecadado = 500.00m
        };

        var campanhaConcluida = new Campanha
        {
            Id = Guid.NewGuid(),
            Titulo = "Campanha Concluída",
            Descricao = "Descrição",
            MetaFinanceira = 1000.00m,
            Status = Status.Concluido,
            ValorArrecadado = 1000.00m
        };

        _context.Campanhas.AddRange(campanhaAtiva, campanhaConcluida);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.ObterCampanhasAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Campanha Ativa", result.First().Titulo);
    }

    [Fact]
    public async Task ObterCampanhasAsync_DeveRetornarListaVazia_QuandoNaoHaCampanhasAtivas()
    {
        // Arrange
        var campanhaConcluida = new Campanha
        {
            Id = Guid.NewGuid(),
            Titulo = "Campanha Concluída",
            Descricao = "Descrição",
            MetaFinanceira = 1000.00m,
            Status = Status.Concluido,
            ValorArrecadado = 1000.00m
        };

        _context.Campanhas.Add(campanhaConcluida);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.ObterCampanhasAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarCampanha_QuandoIdExiste()
    {
        // Arrange
        var campanha = new Campanha
        {
            Id = Guid.NewGuid(),
            Titulo = "Título Original",
            Descricao = "Descrição original",
            MetaFinanceira = 1000.00m,
            Status = Status.Ativo
        };
        _context.Campanhas.Add(campanha);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new AtualizarCampanhaDTO
        {
            Titulo = "Título Atualizado",
            Descricao = "Descrição atualizada",
            MetaFinanceira = 2000.00m,
            Status = Status.Cancelado
        };

        // Act
        var result = await _service.AtualizarAsync(campanha.Id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Título Atualizado", result.Titulo);
        Assert.Equal("Descrição atualizada", result.Descricao);
        Assert.Equal(2000.00m, result.MetaFinanceira);
        Assert.Equal(Status.Cancelado, result.Status);
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarNulo_QuandoIdNaoExiste()
    {
        // Arrange
        var dto = new AtualizarCampanhaDTO
        {
            Titulo = "Título Atualizado",
            Descricao = "Descrição atualizada",
            MetaFinanceira = 2000.00m
        };

        // Act
        var result = await _service.AtualizarAsync(Guid.NewGuid(), dto);

        // Assert
        Assert.Null(result);
    }
}
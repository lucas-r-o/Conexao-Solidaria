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
    private readonly ICampaignService _service;

    public CampanhaServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(serviceOptions => serviceOptions.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddServices();

        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _service = _scope.ServiceProvider.GetRequiredService<ICampaignService>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    [Fact]
    public async Task CreateAsync_DeveCriarCampanha_ComDadosValidos()
    {
        // Arrange
        var dto = new CreateCampaignDto
        {
            Title = "Minha Campaign",
            Description = "Descrição da campaign",
            FinancialGoal = 1000.00m,
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Minha Campaign", result.Titulo);
        Assert.Equal("Descrição da campaign", result.Descricao);
        Assert.Equal(1000.00m, result.MetaFinanceira);
        Assert.Equal(Status.Active, result.Status);
        Assert.Equal(0, result.ValorArrecadado);
        Assert.NotEqual(default, result.DataInicio);
        Assert.NotNull(result.DataFim);
    }

    [Fact]
    public async Task CreateAsync_DeveIncluirCampanhaNoBanco()
    {
        // Arrange
        var dto = new CreateCampaignDto
        {
            Title = "Campaign para Teste",
            Description = "Teste de inserção",
            FinancialGoal = 500.00m,
            EndDate = DateTime.UtcNow.AddDays(15)
        };

        // Act
        await _service.CreateAsync(dto);
        var campanhas = await _context.Campanhas.ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(campanhas);
        Assert.Equal("Campaign para Teste", campanhas[0].Titulo);
    }

    [Fact]
    public async Task GetCampaignsAsync_DeveRetornarGetActiveCampaigns()
    {
        // Arrange
        var campanhaAtiva = new Campaign
        {
            Id = Guid.NewGuid(),
            Titulo = "Campaign Ativa",
            Descricao = "Descrição",
            MetaFinanceira = 1000.00m,
            Status = Status.Active,
            ValorArrecadado = 500.00m
        };

        var campanhaConcluida = new Campaign
        {
            Id = Guid.NewGuid(),
            Titulo = "Campaign Concluída",
            Descricao = "Descrição",
            MetaFinanceira = 1000.00m,
            Status = Status.Completed,
            ValorArrecadado = 1000.00m
        };

        _context.Campanhas.AddRange(campanhaAtiva, campanhaConcluida);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetCampaignsAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Campaign Ativa", result.First().Title);
    }

    [Fact]
    public async Task GetCampaignsAsync_DeveRetornarListaVazia_QuandoNaoHaGetActiveCampaigns()
    {
        // Arrange
        var campanhaConcluida = new Campaign
        {
            Id = Guid.NewGuid(),
            Titulo = "Campaign Concluída",
            Descricao = "Descrição",
            MetaFinanceira = 1000.00m,
            Status = Status.Completed,
            ValorArrecadado = 1000.00m
        };

        _context.Campanhas.Add(campanhaConcluida);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetCampaignsAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarCampanha_QuandoIdExiste()
    {
        // Arrange
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Titulo = "Título Original",
            Descricao = "Descrição original",
            MetaFinanceira = 1000.00m,
            Status = Status.Active
        };
        _context.Campanhas.Add(campaign);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new UpdateCampaignDto
        {
            Title = "Título Atualizado",
            Description = "Descrição atualizada",
            FinancialGoal = 2000.00m,
            Status = Status.Cancelled
        };

        // Act
        var result = await _service.UpdateAsync(campaign.Id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Título Atualizado", result.Titulo);
        Assert.Equal("Descrição atualizada", result.Descricao);
        Assert.Equal(2000.00m, result.MetaFinanceira);
        Assert.Equal(Status.Cancelled, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_DeveRetornarNulo_QuandoIdNaoExiste()
    {
        // Arrange
        var dto = new UpdateCampaignDto
        {
            Title = "Título Atualizado",
            Description = "Descrição atualizada",
            FinancialGoal = 2000.00m
        };

        // Act
        var result = await _service.UpdateAsync(Guid.NewGuid(), dto);

        // Assert
        Assert.Null(result);
    }
}
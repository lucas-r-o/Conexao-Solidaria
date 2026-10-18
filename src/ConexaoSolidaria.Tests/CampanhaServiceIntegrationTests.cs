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
        Assert.Equal("Minha Campaign", result.Title);
        Assert.Equal("Descrição da campaign", result.Description);
        Assert.Equal(1000.00m, result.FinancialGoal);
        Assert.Equal(Status.Active, result.Status);
        Assert.Equal(0, result.AmountRaised);
        Assert.NotEqual(default, result.StartDate);
        Assert.NotNull(result.EndDate);
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
        var campanhas = await _context.Campaigns.ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(campanhas);
        Assert.Equal("Campaign para Teste", campanhas[0].Title);
    }

    [Fact]
    public async Task GetCampaignsAsync_DeveRetornarGetActiveCampaigns()
    {
        // Arrange
        var campanhaAtiva = new Campaign
        {
            Id = Guid.NewGuid(),
            Title = "Campaign Ativa",
            Description = "Descrição",
            FinancialGoal = 1000.00m,
            Status = Status.Active,
            AmountRaised = 500.00m
        };

        var campanhaConcluida = new Campaign
        {
            Id = Guid.NewGuid(),
            Title = "Campaign Concluída",
            Description = "Descrição",
            FinancialGoal = 1000.00m,
            Status = Status.Completed,
            AmountRaised = 1000.00m
        };

        _context.Campaigns.AddRange(campanhaAtiva, campanhaConcluida);
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
            Title = "Campaign Concluída",
            Description = "Descrição",
            FinancialGoal = 1000.00m,
            Status = Status.Completed,
            AmountRaised = 1000.00m
        };

        _context.Campaigns.Add(campanhaConcluida);
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
            Title = "Título Original",
            Description = "Descrição original",
            FinancialGoal = 1000.00m,
            Status = Status.Active
        };
        _context.Campaigns.Add(campaign);
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
        Assert.Equal("Título Atualizado", result.Title);
        Assert.Equal("Descrição atualizada", result.Description);
        Assert.Equal(2000.00m, result.FinancialGoal);
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
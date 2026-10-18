using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using ApplicationServices.Interfaces;
using Infrastructure.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.DTOs;

namespace ConexaoSolidaria.Tests;

public class CampanhasFunctionalTests : IClassFixture<CampanhasApiFactory>
{
    private readonly CampanhasApiFactory _factory;
    private readonly HttpClient _client;

    public CampanhasFunctionalTests(CampanhasApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCampaigns_DeveRetornarSomenteGetActiveCampaigns()
    {
        var activeId = Guid.NewGuid();
        var completedId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Campaigns.AddRange(
                new Campaign
                {
                    Id = activeId,
                    Title = "Campaign funcional ativa",
                    Description = "Descrição de teste",
                    FinancialGoal = 1000m,
                    Status = Status.Active
                },
                new Campaign
                {
                    Id = completedId,
                    Title = "Campaign funcional concluída",
                    Description = "Descrição de teste",
                    FinancialGoal = 1000m,
                    Status = Status.Completed
                });
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var response = await _client.GetAsync("/campaigns", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = System.Text.Json.JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var returnedIds = body.RootElement.EnumerateArray()
            .Select(campaign => campaign.GetProperty("id").GetGuid())
            .ToArray();

        Assert.Contains(activeId, returnedIds);
        Assert.DoesNotContain(completedId, returnedIds);
    }

    [Fact]
    public async Task PostCampaignsCreate_DeveRetornar201EPersistir_QuandoDadosValidos()
    {
        var dto = new CreateCampaignDto
        {
            Title = "Campaign funcional criada",
            Description = "Criada pelo endpoint HTTP",
            FinancialGoal = 750m,
            EndDate = DateTime.UtcNow.AddDays(10)
        };

        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/campaigns/criar");
        request.Content = JsonContent.Create(dto);
        using var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Campaign>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.Equal(dto.Title, created.Title);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var persisted = await context.Campaigns.SingleAsync(
            campaign => campaign.Id == created.Id,
            TestContext.Current.CancellationToken);
        Assert.Equal(dto.Title, persisted.Title);
    }

    [Fact]
    public async Task PostCampaignsCreate_DeveRetornar400_QuandoDadosInvalidos()
    {
        var dto = new CreateCampaignDto
        {
            Title = string.Empty,
            Description = string.Empty,
            FinancialGoal = 0
        };

        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/campaigns/criar");
        request.Content = JsonContent.Create(dto);
        using var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutCampanha_DeveRetornar404_QuandoIdNaoExiste()
    {
        var dto = new UpdateCampaignDto
        {
            Title = "Campaign atualizada",
            Description = "Descrição atualizada",
            FinancialGoal = 900m,
            Status = Status.Active
        };

        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"/campaigns/{Guid.NewGuid()}");
        request.Content = JsonContent.Create(dto);
        using var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string path)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(CampanhasApiFactory.JwtTestKey));
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Role, nameof(Roles.NGOManager))],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token));
        return request;
    }
}

public sealed class CampanhasApiFactory : WebApplicationFactory<Program>
{
    public const string JwtTestKey = "functional-test-key-with-at-least-32-bytes";
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CONEXAO_SOLIDARIA"] = "Host=localhost;Database=tests;Username=tests;Password=tests",
                ["Jwt:Key"] = JwtTestKey,
                ["Bus:Host"] = "localhost",
                ["Bus:Port"] = "5672",
                ["Bus:User"] = "guest",
                ["Bus:Password"] = "guest"
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(JwtTestKey)));

            var massTransitHostedServices = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                    && descriptor.ImplementationType?.Assembly == typeof(IPublishEndpoint).Assembly)
                .ToArray();

            foreach (var hostedService in massTransitHostedServices)
                services.Remove(hostedService);
        });
    }
}
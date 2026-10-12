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
    public async Task GetCampanhas_DeveRetornarSomenteCampanhasAtivas()
    {
        var activeId = Guid.NewGuid();
        var completedId = Guid.NewGuid();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Campanhas.AddRange(
                new Campanha
                {
                    Id = activeId,
                    Titulo = "Campanha funcional ativa",
                    Descricao = "Descrição de teste",
                    MetaFinanceira = 1000m,
                    Status = Status.Ativo
                },
                new Campanha
                {
                    Id = completedId,
                    Titulo = "Campanha funcional concluída",
                    Descricao = "Descrição de teste",
                    MetaFinanceira = 1000m,
                    Status = Status.Concluido
                });
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var response = await _client.GetAsync("/campanhas", TestContext.Current.CancellationToken);

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
    public async Task PostCampanhasCriar_DeveRetornar201EPersistir_QuandoDadosValidos()
    {
        var dto = new CriarCampanhaDTO
        {
            Titulo = "Campanha funcional criada",
            Descricao = "Criada pelo endpoint HTTP",
            MetaFinanceira = 750m,
            DataFim = DateTime.UtcNow.AddDays(10)
        };

        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/campanhas/criar");
        request.Content = JsonContent.Create(dto);
        using var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Campanha>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.Equal(dto.Titulo, created.Titulo);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var persisted = await context.Campanhas.SingleAsync(
            campaign => campaign.Id == created.Id,
            TestContext.Current.CancellationToken);
        Assert.Equal(dto.Titulo, persisted.Titulo);
    }

    [Fact]
    public async Task PostCampanhasCriar_DeveRetornar400_QuandoDadosInvalidos()
    {
        var dto = new CriarCampanhaDTO
        {
            Titulo = string.Empty,
            Descricao = string.Empty,
            MetaFinanceira = 0
        };

        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/campanhas/criar");
        request.Content = JsonContent.Create(dto);
        using var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutCampanha_DeveRetornar404_QuandoIdNaoExiste()
    {
        var dto = new AtualizarCampanhaDTO
        {
            Titulo = "Campanha atualizada",
            Descricao = "Descrição atualizada",
            MetaFinanceira = 900m,
            Status = Status.Ativo
        };

        using var request = CreateAuthorizedRequest(HttpMethod.Put, $"/campanhas/{Guid.NewGuid()}");
        request.Content = JsonContent.Create(dto);
        using var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string path)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(CampanhasApiFactory.JwtTestKey));
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Role, nameof(Roles.GestorONG))],
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
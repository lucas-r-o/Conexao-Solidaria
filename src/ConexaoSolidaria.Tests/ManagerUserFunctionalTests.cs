using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.Dtos;

namespace ConexaoSolidaria.Tests;

public class ManagerUserFunctionalTests : IClassFixture<CampanhasApiFactory>
{
    private readonly CampanhasApiFactory _factory;
    private readonly HttpClient _client;

    public ManagerUserFunctionalTests(CampanhasApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateManager_DeveExigirRoleNGOManager()
    {
        var response = await _client.PostAsJsonAsync(
            "/usuarios/gestores",
            CreateRegistrationDto("unauthorized@example.com"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateManager_DeveCriarUsuarioComRoleNGOManager()
    {
        var user = CreateRegistrationDto($"manager-{Guid.NewGuid():N}@example.com");
        using var request = CreateManagerRequest(user);
        using var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(
            "Usuário cadastrado com sucesso",
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var persistedUser = await context.Usuarios.SingleAsync(
            existing => existing.Email == user.Email,
            TestContext.Current.CancellationToken);
        Assert.Equal(Roles.NGOManager, persistedUser.Role);
    }

    private static RegistrationDto CreateRegistrationDto(string email) => new()
    {
        Name = "Organization Manager",
        Email = email,
        TaxId = "11144477735",
        PasswordHash = "Senha123!"
    };

    private static HttpRequestMessage CreateManagerRequest(RegistrationDto user)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(CampanhasApiFactory.JwtTestKey));
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Role, nameof(Roles.NGOManager))],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var request = new HttpRequestMessage(HttpMethod.Post, "/usuarios/gestores")
        {
            Content = JsonContent.Create(user)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token));
        return request;
    }
}

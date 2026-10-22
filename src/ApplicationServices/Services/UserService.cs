using ApplicationServices.Interfaces;
using ConexaoSolidaria.Events;
using Infrastructure;
using Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.Dtos;
using Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApplicationServices.Services;

internal class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPublishEndpoint _published;
    private readonly IConfiguration _configuration;
    private const string MESSAGE_ERROR = "E-mail ou senha inválidos";
    private const string JWT_KEY = "Jwt:Key";

    public UserService(AppDbContext context, IPublishEndpoint published, IConfiguration configuration)
    {
        _context = context;
        _published = published;
        _configuration = configuration;
    }

    public async Task<Result<string>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email, cancellationToken);

        if (user == null || !UserHelper.IsValidPassword(loginDto.Password, user.SenhaHash))
            return Result<string>.Failure(MESSAGE_ERROR);

        return Result<string>.Success(GenerateToken(user));
    }

    public Task<Result<string>> RegisterAsync(RegistrationDto userDto, CancellationToken cancellationToken) => RegisterAsync(userDto, Roles.Donor, cancellationToken);

    public Task<Result<string>> RegisterManagerAsync(RegistrationDto userDto, CancellationToken cancellationToken) => RegisterAsync(userDto, Roles.NGOManager, cancellationToken);

    private async Task<Result<string>> RegisterAsync(RegistrationDto userDto, Roles role, CancellationToken cancellationToken)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email == userDto.Email, cancellationToken))
            return Result<string>.Failure("E-mail já cadastrado.");

        var user = new User()
        {
            Nome = userDto.Name,
            Email = userDto.Email,
            CPF = userDto.TaxId,
            SenhaHash = UserHelper.HashPassword(userDto.PasswordHash),
            Role = role,
        };

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Usuário cadastrado com sucesso");
    }

    public async Task<bool> TryDonationAsync(DonationDto donation, CancellationToken cancellationToken = default)
    {
        var goal = await _context.Campanhas.Where(p => p.Status == Status.Active && p.Id == donation.CampaignId)
                                     .Select(p => (decimal?)p.MetaFinanceira)
                                     .FirstOrDefaultAsync(cancellationToken);
        if (goal == null) return false;

        await _published.Publish(new DoacaoRecebidaEvent
        {
            CampanhaId = donation.CampaignId,
            DoadorId = donation.DonorId,
            Valor = donation.Amount,
            MetaFinanceira = goal.Value
        }, cancellationToken);

        _ = await _context.Doacoes.AddAsync(new()
        {
            Valor = donation.Amount,
            UsuarioId = donation.DonorId,
            CampanhaId = donation.CampaignId,
        }, cancellationToken);

        _ = await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyString = _configuration.GetValue<string>(JWT_KEY) ?? throw new ArgumentException(JWT_KEY);
        var key = Encoding.ASCII.GetBytes(keyString);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

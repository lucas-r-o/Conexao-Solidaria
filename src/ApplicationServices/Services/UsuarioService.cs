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

internal class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;
    private readonly IPublishEndpoint _published;
    private readonly IConfiguration _configuration;
    private const string MESSAGE_ERROR = "E-mail ou senha inválidos";
    private const string JWT_KEY = "Jwt:Key";

    public UsuarioService(AppDbContext context, IPublishEndpoint published, IConfiguration configuration)
    {
        _context = context;
        _published = published;
        _configuration = configuration;
    }

    public async Task<Result<string>> LoginAsync(LoginDTO loginDto, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email, cancellationToken);

        if (usuario == null || !UsuarioHelper.IsValidPassword(loginDto.Senha, usuario.SenhaHash))
            return Result<string>.Failure(MESSAGE_ERROR);

        return Result<string>.Success(GerarToken(usuario));
    }

    public async Task<Result<string>> CadastrarAsync(CadastroDto usuarioDto, CancellationToken cancellationToken)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email == usuarioDto.Email, cancellationToken))
            return Result<string>.Failure("E-mail já cadastrado.");

        var usuario = new Usuario()
        {
            Nome = usuarioDto.Nome,
            Email = usuarioDto.Email,
            CPF = usuarioDto.CPF,
            SenhaHash = UsuarioHelper.HashPassword(usuarioDto.SenhaHash),
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Usuário cadastrado com sucesso");
    }

    public async Task<bool> TryDoacaoAsync(DoacaoDto doacao, CancellationToken cancellationToken = default)
    {
        var meta = await _context.Campanhas.Where(p => p.Status == Status.Ativo && p.Id == doacao.CampanhaId)
                                     .Select(p => (decimal?)p.MetaFinanceira)
                                     .FirstOrDefaultAsync(cancellationToken);
        if (meta == null) return false;

        await _published.Publish(new DoacaoRecebidaEvent
        {
            CampanhaId = doacao.CampanhaId,
            DoadorId = doacao.DoadorId,
            Valor = doacao.Valor,
            MetaFinanceira = meta.Value
        }, cancellationToken);

        _ = await _context.Doacoes.AddAsync(new()
        {
            Valor = doacao.Valor,
            UsuarioId = doacao.DoadorId,
            CampanhaId = doacao.CampanhaId,
        }, cancellationToken);

        _ = await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private string GerarToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyString = _configuration.GetValue<string>(JWT_KEY) ?? throw new ArgumentException(JWT_KEY);
        var key = Encoding.ASCII.GetBytes(keyString);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

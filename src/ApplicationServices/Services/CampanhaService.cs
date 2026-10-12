using ApplicationServices.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Dtos;
using Models.DTOs;

namespace ApplicationServices.Services;

internal class CampanhaService : ICampanhaService
{
    private readonly AppDbContext _context;

    public CampanhaService(AppDbContext context) => _context = context;

    public async Task<Campanha> CriarAsync(CriarCampanhaDTO dto)
    {
        var campanha = new Campanha
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            MetaFinanceira = dto.MetaFinanceira,
            Id = Guid.NewGuid(),
            Status = Status.Ativo,
            ValorArrecadado = 0,
            DataInicio = DateTime.UtcNow,
            DataFim = DateTimeSpecifykind(dto.DataFim)
        };

        _ = await _context.Campanhas.AddAsync(campanha);
        _ = await _context.SaveChangesAsync();

        return campanha;
    }
    public async Task<CampanhasDto?> ObterCampanhaPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Campanhas
            .Where(c => c.Status == Status.Ativo && c.Id == id)
            .Select(c => new CampanhasDto
            {
                Id = c.Id,
                Titulo = c.Titulo,
                Descricao = c.Descricao,
                DataFim = c.DataFim,
                Status = c.Status,
                MetaFinanceira = c.MetaFinanceira
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Campanha?> AtualizarAsync(Guid id, AtualizarCampanhaDTO dto)
    {
        var campanha = await _context.Campanhas.FindAsync(id);
        if (campanha == null) return null;

        campanha.Titulo = dto.Titulo;
        campanha.Descricao = dto.Descricao;
        campanha.MetaFinanceira = dto.MetaFinanceira;
        campanha.DataFim = DateTimeSpecifykind(dto.DataFim);
        campanha.Status = dto.Status;

        await _context.SaveChangesAsync();
        return campanha;
    }

    private static DateTime? DateTimeSpecifykind(DateTime? dataFim)
    {
        return dataFim.HasValue
                                ? DateTime.SpecifyKind(dataFim.Value, DateTimeKind.Utc)
                                : null;
    }

    public async Task<IEnumerable<CampanhaPublicaDTO>> ObterCampanhasAsync(CancellationToken cancellationToken)
    {
        return await _context.Campanhas
            .Where(c => c.Status == Status.Ativo)
            .Select(c => new CampanhaPublicaDTO
            {
                Id = c.Id,
                Titulo = c.Titulo,
                MetaFinanceira = c.MetaFinanceira,
                ValorTotalArrecadado = c.ValorArrecadado
            })
            .ToListAsync(cancellationToken);
    }
}
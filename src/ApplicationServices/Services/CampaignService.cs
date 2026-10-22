using ApplicationServices.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Dtos;
using Models.DTOs;

namespace ApplicationServices.Services;

internal class CampaignService : ICampaignService
{
    private readonly AppDbContext _context;

    public CampaignService(AppDbContext context) => _context = context;

    public async Task<Campaign> CreateAsync(CreateCampaignDto dto)
    {
        var campaign = new Campaign
        {
            Titulo = dto.Title,
            Descricao = dto.Description,
            MetaFinanceira = dto.FinancialGoal,
            Id = Guid.NewGuid(),
            Status = Status.Active,
            ValorArrecadado = 0,
            DataInicio = DateTime.UtcNow,
            DataFim = DateTimeSpecifykind(dto.EndDate)
        };

        _ = await _context.Campanhas.AddAsync(campaign);
        _ = await _context.SaveChangesAsync();

        return campaign;
    }
    public async Task<CampaignsDto?> GetCampaignByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Campanhas
            .Where(c => c.Status == Status.Active && c.Id == id)
            .Select(c => new CampaignsDto
            {
                Id = c.Id,
                Title = c.Titulo,
                Description = c.Descricao,
                EndDate = c.DataFim,
                Status = c.Status,
                FinancialGoal = c.MetaFinanceira
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Campaign?> UpdateAsync(Guid id, UpdateCampaignDto dto)
    {
        var campaign = await _context.Campanhas.FindAsync(id);
        if (campaign == null) return null;

        campaign.Titulo = dto.Title;
        campaign.Descricao = dto.Description;
        campaign.MetaFinanceira = dto.FinancialGoal;
        campaign.DataFim = DateTimeSpecifykind(dto.EndDate);
        campaign.Status = dto.Status;

        await _context.SaveChangesAsync();
        return campaign;
    }

    private static DateTime? DateTimeSpecifykind(DateTime? endDate)
    {
        return endDate.HasValue
                                ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                                : null;
    }

    public async Task<IEnumerable<PublicCampaignDto>> GetCampaignsAsync(CancellationToken cancellationToken)
    {
        return await _context.Campanhas
            .Where(c => c.Status == Status.Active)
            .Select(c => new PublicCampaignDto
            {
                Id = c.Id,
                Title = c.Titulo,
                FinancialGoal = c.MetaFinanceira,
                TotalAmountRaised = c.ValorArrecadado
            })
            .ToListAsync(cancellationToken);
    }
}
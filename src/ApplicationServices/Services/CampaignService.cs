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
            Title = dto.Title,
            Description = dto.Description,
            FinancialGoal = dto.FinancialGoal,
            Id = Guid.NewGuid(),
            Status = Status.Active,
            AmountRaised = 0,
            StartDate = DateTime.UtcNow,
            EndDate = DateTimeSpecifykind(dto.EndDate)
        };

        _ = await _context.Campaigns.AddAsync(campaign);
        _ = await _context.SaveChangesAsync();

        return campaign;
    }
    public async Task<CampaignsDto?> GetCampaignByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Campaigns
            .Where(c => c.Status == Status.Active && c.Id == id)
            .Select(c => new CampaignsDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                EndDate = c.EndDate,
                Status = c.Status,
                FinancialGoal = c.FinancialGoal
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Campaign?> UpdateAsync(Guid id, UpdateCampaignDto dto)
    {
        var campaign = await _context.Campaigns.FindAsync(id);
        if (campaign == null) return null;

        campaign.Title = dto.Title;
        campaign.Description = dto.Description;
        campaign.FinancialGoal = dto.FinancialGoal;
        campaign.EndDate = DateTimeSpecifykind(dto.EndDate);
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
        return await _context.Campaigns
            .Where(c => c.Status == Status.Active)
            .Select(c => new PublicCampaignDto
            {
                Id = c.Id,
                Title = c.Title,
                FinancialGoal = c.FinancialGoal,
                TotalAmountRaised = c.AmountRaised
            })
            .ToListAsync(cancellationToken);
    }
}
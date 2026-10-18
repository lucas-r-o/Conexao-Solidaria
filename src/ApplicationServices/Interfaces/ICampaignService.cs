using Models;
using Models.Dtos;
using Models.DTOs;

namespace ApplicationServices.Interfaces;

public interface ICampaignService
{
    Task<Campaign> CreateAsync(CreateCampaignDto campaign);
    Task<Campaign?> UpdateAsync(Guid id, UpdateCampaignDto dto);
    Task<IEnumerable<PublicCampaignDto>> GetCampaignsAsync(CancellationToken cancellationToken);
    Task<CampaignsDto?> GetCampaignByIdAsync(Guid id, CancellationToken cancellationToken);
}
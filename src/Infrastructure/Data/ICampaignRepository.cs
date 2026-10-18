using ConexaoSolidaria.Events;

namespace Infrastructure.Data
{
    public interface ICampaignRepository
    {
        Task<bool> UpdateAsync(ArrecadacaoAtualizadaEvent campaignUpdated);
    }
}

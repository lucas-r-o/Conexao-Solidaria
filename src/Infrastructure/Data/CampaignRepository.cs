using ConexaoSolidaria.Events;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Infrastructure.Data
{
    internal class CampaignRepository(AppDbContext db) : ICampaignRepository
    {
        private readonly AppDbContext _db = db;
        public async Task<bool> UpdateAsync(ArrecadacaoAtualizadaEvent campaignUpdated)
        {
            var campaign = await _db.Campanhas.FirstOrDefaultAsync(p => p.Id == campaignUpdated.CampanhaId);

            if (campaign == null) return false;

            campaign.ValorArrecadado = campaignUpdated.TotalArrecadado;

            if (campaign.ValorArrecadado >= campaign.MetaFinanceira)
                campaign.Status = Status.Completed;

            _db.Campanhas.Update(campaign);
            var result = await _db.SaveChangesAsync();

            return result > 0;
        }
    }
}

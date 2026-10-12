using ConexaoSolidaria.Events;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Infrastructure.Data
{
    internal class CampanhaRepository(AppDbContext db) : ICampanhaRepository
    {
        private readonly AppDbContext _db = db;
        public async Task<bool> UpdateAsync(ArrecadacaoAtualizadaEvent campanhaAtualizada)
        {
            var campanha = await _db.Campanhas.FirstOrDefaultAsync(p => p.Id == campanhaAtualizada.CampanhaId);

            if (campanha == null) return false;

            campanha.ValorArrecadado = campanhaAtualizada.TotalArrecadado;

            if (campanha.ValorArrecadado >= campanha.MetaFinanceira)
                campanha.Status = Status.Concluido;

            _db.Campanhas.Update(campanha);
            var result = await _db.SaveChangesAsync();

            return result > 0;
        }
    }
}

using ConexaoSolidaria.Events;

namespace Infrastructure.Data
{
    public interface ICampanhaRepository
    {
        Task<bool> UpdateAsync(ArrecadacaoAtualizadaEvent campanhaAtualizada);
    }
}

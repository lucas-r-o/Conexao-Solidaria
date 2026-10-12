using Models;
using Models.Dtos;
using Models.DTOs;

namespace ApplicationServices.Interfaces;

public interface ICampanhaService
{
    Task<Campanha> CriarAsync(CriarCampanhaDTO campanha);
    Task<Campanha?> AtualizarAsync(Guid id, AtualizarCampanhaDTO dto);
    Task<IEnumerable<CampanhaPublicaDTO>> ObterCampanhasAsync(CancellationToken cancellationToken);
    Task<CampanhasDto?> ObterCampanhaPorIdAsync(Guid id, CancellationToken cancellationToken);
}
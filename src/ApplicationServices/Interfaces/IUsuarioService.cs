using Models.Dtos;
using Models.DTOs;

namespace ApplicationServices.Interfaces;

public interface IUsuarioService
{
    Task<Result<string>> LoginAsync(LoginDTO loginDto, CancellationToken cancellationToken);
    Task<Result<string>> CadastrarAsync(CadastroDto usuario, CancellationToken cancellationToken);
    Task<bool> TryDoacaoAsync(DoacaoDto doacao, CancellationToken cancellationToken = default);
}
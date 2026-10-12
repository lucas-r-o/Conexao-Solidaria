using ApplicationServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
[Route("doadores")]
public class DoadoresController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public DoadoresController(IUsuarioService usuarioService) => _usuarioService = usuarioService;

    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroDto usuario, CancellationToken cancellationToken)
    {
        var novoUsuario = await _usuarioService.CadastrarAsync(usuario, cancellationToken);

        return !novoUsuario.IsSuccess ? BadRequest(novoUsuario.Value) : CreatedAtAction(nameof(Cadastrar), novoUsuario.Value);
    }
}

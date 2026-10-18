using ApplicationServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Dtos;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
public class DoadoresController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public DoadoresController(IUsuarioService usuarioService) => _usuarioService = usuarioService;

    [Route("doadores")]
    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroDto usuario, CancellationToken cancellationToken)
    {
        var novoUsuario = await _usuarioService.CadastrarAsync(usuario, cancellationToken);

        return !novoUsuario.IsSuccess ? BadRequest(novoUsuario.Value) : CreatedAtAction(nameof(Cadastrar), novoUsuario.Value);
    }

    [HttpPost("/doacoes")]
    [Authorize(Roles = nameof(Roles.Doador))]
    public async Task<IActionResult> Doar([FromBody] DoacaoDto doacao, CancellationToken cancellationToken)
    {
        var isSent = await _usuarioService.TryDoacaoAsync(doacao, cancellationToken);

        return !isSent ? BadRequest("Campanha inativa ou inexistente.") : Ok(new
        {
            Enviado = isSent,
            Mensagem = "Processando doação, resultado enviado no e-mail!"
        });
    }
}

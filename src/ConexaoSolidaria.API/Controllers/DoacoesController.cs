using ApplicationServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Dtos;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
[Route("doacoes")]
[Authorize(Roles = nameof(Roles.Doador))]
public class DoacoesController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public DoacoesController(IUsuarioService usuarioService) => _usuarioService = usuarioService;

    [HttpPost]
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

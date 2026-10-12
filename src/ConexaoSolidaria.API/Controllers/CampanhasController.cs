using ApplicationServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
[Route("campanhas")]
public class CampanhasController : ControllerBase
{
    private readonly ICampanhaService _campanhaService;

    public CampanhasController(ICampanhaService campanhaService) => _campanhaService = campanhaService;

    [HttpPost("criar")]
    [Authorize(Roles = nameof(Roles.GestorONG))]
    public async Task<IActionResult> Criar([FromBody] CriarCampanhaDTO dto)
    {
        var campanha = await _campanhaService.CriarAsync(dto);

        return CreatedAtAction(nameof(Criar), new { id = campanha.Id }, campanha);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Roles.GestorONG))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCampanhaDTO dto)
    {
        var campanha = await _campanhaService.AtualizarAsync(id, dto);
        if (campanha == null)
            return NotFound("Campanha não encontrada.");

        return Ok(campanha);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CampanhasAtivas(CancellationToken cancellationToken)
    {
        var campanhas = await _campanhaService.ObterCampanhasAsync(cancellationToken);
        return Ok(campanhas);
    }

    [HttpGet]
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> CampanhasAtivasPorId(Guid id, CancellationToken cancellationToken)
    {
        var campanha = await _campanhaService.ObterCampanhaPorIdAsync(id, cancellationToken);
        if (campanha == null)
            return NotFound("Campanha não encontrada.");

        return Ok(campanha);
    }
}
using ApplicationServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
[Route("campaigns")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campanhaService;

    public CampaignsController(ICampaignService campanhaService) => _campanhaService = campanhaService;

    [HttpPost("criar")]
    [Authorize(Roles = nameof(Roles.NGOManager))]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
    {
        var campaign = await _campanhaService.CreateAsync(dto);

        return CreatedAtAction(nameof(Create), new { id = campaign.Id }, campaign);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Roles.NGOManager))]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignDto dto)
    {
        var campaign = await _campanhaService.UpdateAsync(id, dto);
        if (campaign == null)
            return NotFound("Campaign não encontrada.");

        return Ok(campaign);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveCampaigns(CancellationToken cancellationToken)
    {
        var campaigns = await _campanhaService.GetCampaignsAsync(cancellationToken);
        return Ok(campaigns);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetActiveCampaignById(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await _campanhaService.GetCampaignByIdAsync(id, cancellationToken);
        if (campaign == null)
            return NotFound("Campaign não encontrada.");

        return Ok(campaign);
    }
}
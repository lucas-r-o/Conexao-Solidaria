using ApplicationServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Dtos;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
public class DonorsController : ControllerBase
{
    private readonly IUserService _usuarioService;

    public DonorsController(IUserService usuarioService) => _usuarioService = usuarioService;

    [Route("donors")]
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegistrationDto user, CancellationToken cancellationToken)
    {
        var newUser = await _usuarioService.RegisterAsync(user, cancellationToken);

        return !newUser.IsSuccess ? BadRequest(newUser.Value) : CreatedAtAction(nameof(Register), newUser.Value);
    }

    [HttpPost("/donations")]
    [Authorize(Roles = nameof(Roles.Donor))]
    public async Task<IActionResult> Donate([FromBody] DonationDto donation, CancellationToken cancellationToken)
    {
        var isSent = await _usuarioService.TryDonationAsync(donation, cancellationToken);

        return !isSent ? BadRequest("Campaign inativa ou inexistente.") : Ok(new
        {
            Enviado = isSent,
            Message = "Processando doação, resultado enviado no e-mail!"
        });
    }
}

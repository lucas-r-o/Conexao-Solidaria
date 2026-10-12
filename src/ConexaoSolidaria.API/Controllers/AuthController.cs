using ApplicationServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _services;

    public AuthController(IUsuarioService services) => _services = services;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO, CancellationToken cancellationToken)
    {
        var login = await _services.LoginAsync(loginDTO, cancellationToken);

        return !login.IsSuccess ? BadRequest(login.Error) : Ok(login.Value);
    }
}
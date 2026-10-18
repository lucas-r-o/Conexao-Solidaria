using ApplicationServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _services;

    public AuthController(IUserService services) => _services = services;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDTO, CancellationToken cancellationToken)
    {
        var login = await _services.LoginAsync(loginDTO, cancellationToken);

        return !login.IsSuccess ? BadRequest(login.Error) : Ok(login.Value);
    }
}
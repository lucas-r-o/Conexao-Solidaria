using ApplicationServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Dtos;

namespace ConexaoSolidaria.API.Controllers;

[ApiController]
[Route("usuarios")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpPost("gestores")]
    [Authorize(Roles = nameof(Roles.NGOManager))]
    public async Task<IActionResult> CreateManager(
        [FromBody] RegistrationDto user,
        CancellationToken cancellationToken)
    {
        var newUser = await _userService.RegisterManagerAsync(user, cancellationToken);

        return !newUser.IsSuccess
            ? BadRequest(newUser.Error)
            : CreatedAtAction(nameof(CreateManager), newUser.Value);
    }
}

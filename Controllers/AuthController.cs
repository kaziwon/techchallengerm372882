using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Controllers.Mappers;
using OficinaMecanica.Api.InterfaceAdapters.Controllers;
using OficinaMecanica.Api.InterfaceAdapters.DTOs;
using HttpDtos = OficinaMecanica.Api.Controllers.DTOs;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AuthCleanController _authCleanController;

    public AuthController(AuthCleanController authCleanController)
    {
        _authCleanController = authCleanController;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] HttpDtos.LoginRequestDto loginRequestDto)
    {
        var response = _authCleanController.Login(loginRequestDto.ParaCleanDto());

        return response is null ? Unauthorized() : Ok(response);
    }
}

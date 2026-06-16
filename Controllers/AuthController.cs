using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.UseCases.Auth;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;

    public AuthController(LoginUseCase loginUseCase)
    {
        _loginUseCase = loginUseCase;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var output = _loginUseCase.Executar(new LoginInput(loginRequestDto.Username, loginRequestDto.Password));
        var response = output is null
            ? null
            : new LoginResponseDto
            {
                Token = output.Token,
                ExpiresAt = output.ExpiresAt
            };

        return response is null ? Unauthorized() : Ok(response);
    }
}

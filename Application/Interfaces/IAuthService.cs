using OficinaMecanica.Api.Application.DTOs;

namespace OficinaMecanica.Api.Application.Interfaces;

public interface IAuthService
{
    LoginResponseDto? Login(LoginRequestDto loginRequestDto);
}

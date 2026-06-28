using Application.DTO;
using Application.Services.UserServices;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserService userService, JWTService jwtService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserAuthDto userAuthDto)
    {
        UserRoles role = userAuthDto.Role ?? UserRoles.User;
        var user = await userService.RegisterUserAsync(userAuthDto.Login, userAuthDto.Password, userAuthDto.Role);
        return Ok(true);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserAuthDto userAuthDto)
    {
        var userInfo = await userService.SignInUserAsync(userAuthDto.Login, userAuthDto.Password);
        var token = jwtService.GenerateToken(userInfo);
        return Ok(token);
    }
}
using Application.DTO;
using Application.Services.UserServices;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserService userService, IJWTService jwtService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserAuthDto userAuthDto)
    {
        UserRoles role = userAuthDto.Role ?? UserRoles.User;
        var user = await userService.RegisterUserAsync(userAuthDto.Login, userAuthDto.Password, userAuthDto.Role);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        var userInfo = await userService.SignInUserAsync(userLoginDto.Login, userLoginDto.Password);
        var token = jwtService.GenerateToken(userInfo);
        return Ok(token);
    }
}
using EMR.Application.DTOs.Auth;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMR.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (result is null)
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        if (result is null)
            return Unauthorized(new { message = "Your 30-day session or refresh token has expired. Please log in again." });

        return Ok(result);
    }
}
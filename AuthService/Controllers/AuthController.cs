namespace AuthServicex.Controllers;

// AuthService/Controllers/AuthController.cs
using AuthServicex.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request.Email, request.Password);
        return Ok(new { user.Id, user.Email });
    }

    [HttpGet("verify")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email)
    {
        var result = await _authService.VerifyEmailAsync(email);
        if (result) return Ok("Email verified successfully.");
        return BadRequest("Verification failed.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);
        return Ok(new { Token = token });
    }
}

// Request Models
public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);


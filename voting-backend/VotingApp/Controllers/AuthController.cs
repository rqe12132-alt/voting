using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingApp.DTOs.Auth;
using VotingApp.Services;

namespace VotingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(new { message = "Неверный email или пароль" });
        }
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokensAsync(request.RefreshToken);
        if (result == null)
        {
            return Unauthorized(new { message = "Недействительный refresh token" });
        }
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(userId.Value);
        if (user == null) return Unauthorized();

        return Ok(user);
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code))
        {
            return BadRequest(new { message = "Email и код подтверждения обязательны" });
        }

        if (request.Code.Length != 6 || !request.Code.All(char.IsDigit))
        {
            return BadRequest(new { message = "Код подтверждения должен состоять из 6 цифр" });
        }

        var result = await _authService.VerifyEmailAsync(request.Email, request.Code);
        if (result)
        {
            return Ok(new { message = "Email успешно подтвержден" });
        }
        return BadRequest(new { message = "Неверный или просроченный код подтверждения" });
    }

    [HttpPost("resend-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmail([FromBody] ResendEmailRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new { message = "Email не указан" });
        }

        var result = await _authService.ResendVerificationEmailAsync(request.Email);
        if (result)
        {
            return Ok(new { message = "Письмо с подтверждением отправлено повторно" });
        }
        return BadRequest(new { message = "Пользователь не найден или email уже подтвержден" });
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingApp.Services;

namespace VotingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IAuditService _auditService;

    public UploadController(IWebHostEnvironment env, IAuditService auditService)
    {
        _env = env;
        _auditService = auditService;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Файл не выбран" });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(new { message = "Допустимые форматы: jpg, png, gif, webp, bmp" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Максимальный размер файла — 5 МБ" });

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/uploads/{fileName}";

        var userId = GetUserId();
        var email = GetUserEmail();
        await _auditService.LogAsync(userId, email ?? "", "UPLOAD", "File", fileName, $"Загружен файл: {file.FileName}");

        return Ok(new { url, fileName });
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    private string? GetUserEmail()
    {
        return User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value
            ?? User.FindFirst("email")?.Value;
    }
}

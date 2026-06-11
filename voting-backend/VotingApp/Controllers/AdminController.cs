using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VotingApp.Data;
using VotingApp.DTOs.Auth;
using VotingApp.DTOs.Poll;
using VotingApp.Models;
using VotingApp.Repositories;
using VotingApp.Services;

namespace VotingApp.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IPollService _pollService;
    private readonly IAuditService _auditService;
    private readonly IExcelExportService _excelExportService;
    private readonly IUserRepository _userRepository;
    private readonly IPersonalIdRepository _personalIdRepository;
    private readonly AppDbContext _context;

    public AdminController(IPollService pollService, IAuditService auditService, IExcelExportService excelExportService, IUserRepository userRepository, IPersonalIdRepository personalIdRepository, AppDbContext context)
    {
        _pollService = pollService;
        _auditService = auditService;
        _excelExportService = excelExportService;
        _userRepository = userRepository;
        _personalIdRepository = personalIdRepository;
        _context = context;
    }

    [HttpPost("polls")]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
    {
        if (!IsAdmin()) return Forbid();

        var userId = GetUserId();
        var email = GetUserEmail();
        if (!userId.HasValue) return Unauthorized();

        try
        {
            var poll = await _pollService.CreatePollAsync(userId.Value, request);
            if (poll == null)
            {
                return BadRequest(new { message = "Ошибка создания голосования" });
            }

            await _auditService.LogAsync(userId, email ?? "", "CREATE", "Poll", poll.Id.ToString(), $"Создано голосование: {poll.Title}");
            return Ok(poll);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("polls")]
    public async Task<IActionResult> GetAllPolls()
    {
        if (!IsAdmin()) return Forbid();

        var polls = await _pollService.GetAllPollsAsync();
        return Ok(polls);
    }

    [HttpPut("polls/{id:guid}")]
    public async Task<IActionResult> UpdatePoll(Guid id, [FromBody] UpdatePollRequest request)
    {
        if (!IsAdmin()) return Forbid();

        if (!await _pollService.CanEditAsync(id))
        {
            return BadRequest(new { message = "Редактирование доступно только для черновиков" });
        }

        try
        {
            var poll = await _pollService.UpdatePollAsync(id, request);
            if (poll == null) return NotFound();
            return Ok(poll);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("polls/{id:guid}/publish")]
    public async Task<IActionResult> PublishPoll(Guid id)
    {
        if (!IsAdmin()) return Forbid();

        var userId = GetUserId();
        var email = GetUserEmail();

        var success = await _pollService.PublishPollAsync(id);
        if (!success) return BadRequest(new { message = "Ошибка публикации. Возможно, голосование уже опубликовано или не существует." });

        await _auditService.LogAsync(userId, email ?? "", "PUBLISH", "Poll", id.ToString(), "Голосование опубликовано");
        return Ok(new { message = "Голосование опубликовано" });
    }

    [HttpPost("polls/{id:guid}/close")]
    public async Task<IActionResult> ClosePoll(Guid id)
    {
        if (!IsAdmin()) return Forbid();

        var userId = GetUserId();
        var email = GetUserEmail();

        var success = await _pollService.ClosePollAsync(id);
        if (!success) return BadRequest(new { message = "Ошибка закрытия. Возможно, голосование уже закрыто или не активно." });

        await _auditService.LogAsync(userId, email ?? "", "CLOSE", "Poll", id.ToString(), "Голосование закрыто вручную");
        return Ok(new { message = "Голосование закрыто" });
    }

    [HttpPost("polls/{id:guid}/extend")]
    public async Task<IActionResult> ExtendPoll(Guid id, [FromBody] ExtendPollRequest request)
    {
        if (!IsAdmin()) return Forbid();

        var userId = GetUserId();
        var email = GetUserEmail();

        var success = await _pollService.ExtendPollAsync(id, request.EndsAt);
        if (!success) return BadRequest(new { message = "Ошибка продления. Возможно, голосование уже закрыто." });

        await _auditService.LogAsync(userId, email ?? "", "EXTEND", "Poll", id.ToString(), $"Голосование продлено до: {request.EndsAt?.ToString("dd.MM.yyyy HH:mm") ?? "без срока"}");
        return Ok(new { message = "Срок голосования обновлен" });
    }

    [HttpDelete("polls/{id:guid}")]
    public async Task<IActionResult> DeletePoll(Guid id)
    {
        if (!IsAdmin()) return Forbid();

        var userId = GetUserId();
        var email = GetUserEmail();

        var success = await _pollService.DeletePollAsync(id);
        if (!success) return NotFound();

        await _auditService.LogAsync(userId, email ?? "", "DELETE", "Poll", id.ToString(), "Голосование удалено");
        return Ok(new { message = "Голосование удалено" });
    }

    [HttpGet("polls/{id:guid}/export")]
    public async Task<IActionResult> ExportPoll(Guid id)
    {
        if (!IsAdmin()) return Forbid();

        try
        {
            var bytes = await _excelExportService.ExportPollResultsAsync(id);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"poll-{id}.xlsx");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("users/make-admin")]
    public async Task<IActionResult> MakeAdmin([FromBody] MakeAdminRequest request)
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Нет прав администратора" });

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email обязателен" });
        }

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
        if (user == null)
        {
            return BadRequest(new { message = "Пользователь с таким email не найден" });
        }

        if (user.IsAdmin)
        {
            return BadRequest(new { message = "Пользователь уже является администратором" });
        }

        user.IsAdmin = true;
        await _userRepository.UpdateAsync(user);

        var currentUserId = GetUserId();
        var currentEmail = GetUserEmail();
        await _auditService.LogAsync(currentUserId, currentEmail ?? "", "MAKE_ADMIN", "User", user.Id.ToString(), $"Пользователю {user.Email} назначены права администратора");

        return Ok(new { message = $"Пользователь {user.Email} теперь администратор" });
    }

    [HttpPost("seed-personal-ids")]
    public async Task<IActionResult> SeedPersonalIds()
    {
        if (!IsAdmin()) return StatusCode(403, new { message = "Нет прав администратора" });

        var existingCount = await _personalIdRepository.CountAsync();
        int generatedCount = 0;
        if (existingCount == 0)
        {
            var ids = GeneratePersonalIds(1000);
            await _personalIdRepository.CreateRangeAsync(ids);
            generatedCount = ids.Count;
        }

        // Assign numbers to existing users without one
        var usersWithoutId = await _context.Users
            .Where(u => !_context.PersonalIds.Any(p => p.User != null && p.User.Id == u.Id))
            .ToListAsync();

        var unusedIds = await _personalIdRepository.GetUnusedAsync(usersWithoutId.Count);
        int assignedCount = 0;
        for (int i = 0; i < usersWithoutId.Count && i < unusedIds.Count; i++)
        {
            unusedIds[i].IsUsed = true;
            unusedIds[i].User = usersWithoutId[i];
            await _personalIdRepository.UpdateAsync(unusedIds[i]);
            assignedCount++;
        }

        if (generatedCount > 0)
        {
            return Ok(new { message = $"Сгенерировано {generatedCount} номеров. Назначено {assignedCount} существующим пользователям." });
        }
        else
        {
            return Ok(new { message = $"Назначено {assignedCount} существующим пользователям (свободных номеров: {unusedIds.Count})." });
        }
    }

    private static List<PersonalId> GeneratePersonalIds(int count)
    {
        var random = new Random();
        var regions = new[] { 'A', 'B', 'C', 'H', 'K', 'E', 'M' };
        var citizenships = new[] { "РВ", "BA", "BI" };
        var result = new List<PersonalId>(count);
        var used = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            // Generate birth date 1920-2024
            var year = random.Next(1920, 2025);
            var month = random.Next(1, 13);
            var day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);

            // Determine gender+century code
            int genderCode;
            if (year < 1900) genderCode = random.Next(0, 2) == 0 ? 1 : 2; // XIX (rare in our range, but possible)
            else if (year < 2000) genderCode = random.Next(0, 2) == 0 ? 3 : 4;
            else genderCode = random.Next(0, 2) == 0 ? 5 : 6;

            var dd = day.ToString("D2");
            var mm = month.ToString("D2");
            var yy = (year % 100).ToString("D2");
            var region = regions[random.Next(regions.Length)];
            var serial = (i + 1).ToString("D3"); // 001-999, but we need uniqueness for same day
            var citizenship = citizenships[random.Next(citizenships.Length)];
            var filler = random.Next(0, 10).ToString();

            var number = $"{genderCode}{dd}{mm}{yy}{region}{serial}{citizenship}{filler}";

            if (!used.Add(number))
            {
                i--; // retry
                continue;
            }

            result.Add(new PersonalId { Number = number });
        }

        return result;
    }

    private bool IsAdmin()
    {
        return User.FindFirst("is_admin")?.Value?.ToLower() == "true";
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

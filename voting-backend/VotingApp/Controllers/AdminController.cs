using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingApp.DTOs.Poll;
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

    public AdminController(IPollService pollService, IAuditService auditService, IExcelExportService excelExportService)
    {
        _pollService = pollService;
        _auditService = auditService;
        _excelExportService = excelExportService;
    }

    [HttpPost("polls")]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
    {
        if (!IsAdmin()) return Forbid();

        var userId = GetUserId();
        var email = GetUserEmail();
        if (!userId.HasValue) return Unauthorized();

        var poll = await _pollService.CreatePollAsync(userId.Value, request);
        if (poll == null)
        {
            return BadRequest(new { message = "Ошибка создания голосования" });
        }

        await _auditService.LogAsync(userId, email ?? "", "CREATE", "Poll", poll.Id.ToString(), $"Создано голосование: {poll.Title}");
        return Ok(poll);
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

        var poll = await _pollService.UpdatePollAsync(id, request);
        if (poll == null) return NotFound();
        return Ok(poll);
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

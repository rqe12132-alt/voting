using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingApp.Services;

namespace VotingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;

    public PollsController(IPollService pollService)
    {
        _pollService = pollService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActivePolls([FromQuery] int page = 1, [FromQuery] int pageSize = 6, [FromQuery] bool includeClosed = false)
    {
        var userId = GetUserId();
        var result = await _pollService.GetActivePollsPagedAsync(userId, page, pageSize, includeClosed);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();
        var polls = await _pollService.GetVotedPollsAsync(userId.Value);
        return Ok(polls);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPoll(Guid id)
    {
        var poll = await _pollService.GetPollByIdAsync(id);
        if (poll == null) return NotFound();
        return Ok(poll);
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

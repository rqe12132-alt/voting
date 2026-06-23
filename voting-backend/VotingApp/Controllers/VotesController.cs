using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingApp.DTOs.Vote;
using VotingApp.Services;


namespace VotingApp.Controllers;

[ApiController]
[Route("api/polls/{pollId:guid}/[action]")]
[Authorize]
public class VotesController : ControllerBase
{
    private readonly IVoteService _voteService;
    private readonly IAuditService _auditService;

    public VotesController(IVoteService voteService, IAuditService auditService)
    {
        _voteService = voteService;
        _auditService = auditService;
    }

    [HttpPost]
    public async Task<IActionResult> Vote(Guid pollId, [FromBody] VoteRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var success = await _voteService.VoteAsync(userId.Value, pollId, request);
        if (!success)
        {
            return BadRequest(new { message = "Не удалось проголосовать. Возможно, вы уже голосовали или голосование неактивно." });
        }

        await _auditService.LogAsync(null, "", "VOTE", "Poll", pollId.ToString(), "Анонимный голос");
        return Ok(new { message = "Голос принят" });
    }

    [HttpGet]
    public async Task<IActionResult> Results(Guid pollId)
    {
        var userId = GetUserId();
        var results = await _voteService.GetResultsAsync(pollId, userId);
        if (results == null)
        {
            return Forbid();
        }
        return Ok(results);
    }

    [HttpGet("my-vote")]
    public async Task<IActionResult> MyVote(Guid pollId)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var vote = await _voteService.GetMyVoteAsync(userId.Value, pollId);
        if (vote == null) return NotFound(new { voted = false });
        return Ok(vote);
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

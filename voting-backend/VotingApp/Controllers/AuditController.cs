using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingApp.Services;

namespace VotingApp.Controllers;

[ApiController]
[Route("api/admin/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _auditService.GetAllAsync(page, pageSize);
        var total = await _auditService.CountAsync();
        return Ok(new { items = logs, total, page, pageSize });
    }
}

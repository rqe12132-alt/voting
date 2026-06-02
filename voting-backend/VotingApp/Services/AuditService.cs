using VotingApp.Models;
using VotingApp.Repositories;

namespace VotingApp.Services;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string userEmail, string action, string entityType, string? entityId, string description);
    Task<List<AuditLog>> GetAllAsync(int page, int pageSize);
    Task<int> CountAsync();
}

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repository;

    public AuditService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task LogAsync(Guid? userId, string userEmail, string action, string entityType, string? entityId, string description)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(log);
    }

    public async Task<List<AuditLog>> GetAllAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        return await _repository.GetAllAsync(skip, pageSize);
    }

    public async Task<int> CountAsync()
    {
        return await _repository.CountAsync();
    }
}

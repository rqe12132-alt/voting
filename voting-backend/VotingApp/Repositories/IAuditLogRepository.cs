using VotingApp.Models;

namespace VotingApp.Repositories;

public interface IAuditLogRepository
{
    Task<List<AuditLog>> GetAllAsync(int skip = 0, int take = 100);
    Task<AuditLog> CreateAsync(AuditLog log);
    Task<int> CountAsync();
}

using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;

namespace VotingApp.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuditLog>> GetAllAsync(int skip = 0, int take = 100)
    {
        return await _context.AuditLogs
            .OrderByDescending(e => e.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<AuditLog> CreateAsync(AuditLog log)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<int> CountAsync()
    {
        return await _context.AuditLogs.CountAsync();
    }
}

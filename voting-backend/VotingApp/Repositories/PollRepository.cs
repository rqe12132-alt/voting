using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;

namespace VotingApp.Repositories;

public class PollRepository : IPollRepository
{
    private readonly AppDbContext _context;

    public PollRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Poll?> GetByIdAsync(Guid id)
    {
        return await _context.Polls.FindAsync(id);
    }

    public async Task<Poll?> GetByIdWithOptionsAsync(Guid id)
    {
        return await _context.Polls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Poll?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .ThenInclude(v => v.Option)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Poll>> GetAllAsync()
    {
        return await _context.Polls
            .Include(p => p.Options)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Poll>> GetActiveAsync()
    {
        return await _context.Polls
            .Include(p => p.Options)
            .Where(p => p.Status == PollStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Poll>> GetActivePagedAsync(int skip, int take, bool includeClosed)
    {
        var query = _context.Polls
            .Include(p => p.Options)
            .AsQueryable();

        if (!includeClosed)
        {
            query = query.Where(p => p.Status == PollStatus.Active);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountActiveAsync(bool includeClosed)
    {
        if (!includeClosed)
            return await _context.Polls.CountAsync(p => p.Status == PollStatus.Active);
        return await _context.Polls.CountAsync();
    }

    public async Task<List<Poll>> GetByStatusAsync(PollStatus status)
    {
        return await _context.Polls
            .Include(p => p.Options)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Poll>> GetVotedPollsAsync(Guid userId)
    {
        var pollIds = await _context.Votes
            .Where(v => v.UserId == userId)
            .Select(v => v.PollId)
            .Distinct()
            .ToListAsync();

        return await _context.Polls
            .Include(p => p.Options)
            .Where(p => pollIds.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Poll> CreateAsync(Poll poll)
    {
        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();
        return poll;
    }

    public async Task UpdateAsync(Poll poll)
    {
        _context.Polls.Update(poll);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Poll poll)
    {
        _context.Polls.Remove(poll);
        await _context.SaveChangesAsync();
    }
}

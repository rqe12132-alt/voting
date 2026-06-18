using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;

namespace VotingApp.Repositories;

public class VoteRepository : IVoteRepository
{
    private readonly AppDbContext _context;

    public VoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Vote?> GetByUserAndPollAsync(Guid userId, Guid pollId)
    {
        return await _context.Votes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.PollId == pollId);
    }

    public async Task<List<Vote>> GetByPollAsync(Guid pollId)
    {
        return await _context.Votes
            .Where(v => v.PollId == pollId)
            .ToListAsync();
    }

    public async Task<Vote> CreateAsync(Vote vote)
    {
        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();
        return vote;
    }

    public async Task<int> CountByPollAsync(Guid pollId)
    {
        return await _context.Votes.CountAsync(v => v.PollId == pollId);
    }

    public async Task<int> CountByOptionAsync(Guid optionId)
    {
        return await _context.Votes.CountAsync(v => v.OptionId == optionId);
    }

    public async Task<Dictionary<Guid, int>> GetResultsAsync(Guid pollId)
    {
        return await _context.Votes
            .Where(v => v.PollId == pollId)
            .GroupBy(v => v.OptionId)
            .Select(g => new { OptionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OptionId, x => x.Count);
    }

    public async Task<List<Vote>> GetByPollWithDetailsAsync(Guid pollId)
    {
        return await _context.Votes
            .Where(v => v.PollId == pollId)
            .Include(v => v.User)
            .Include(v => v.Option)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }
}

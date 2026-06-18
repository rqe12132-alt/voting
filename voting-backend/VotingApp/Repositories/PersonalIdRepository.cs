using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;

namespace VotingApp.Repositories;

public class PersonalIdRepository : IPersonalIdRepository
{
    private readonly AppDbContext _context;

    public PersonalIdRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PersonalId?> GetByNumberAsync(string number)
    {
        return await _context.PersonalIds.FirstOrDefaultAsync(p => p.Number == number);
    }

    public async Task<PersonalId?> GetByUserIdAsync(Guid userId)
    {
        return await _context.PersonalIds.FirstOrDefaultAsync(p => p.User != null && p.User.Id == userId);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _context.PersonalIds.AnyAsync(p => p.Number == number);
    }

    public async Task<bool> AnyAsync()
    {
        return await _context.PersonalIds.AnyAsync();
    }

    public async Task CreateAsync(PersonalId personalId)
    {
        _context.PersonalIds.Add(personalId);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PersonalId personalId)
    {
        _context.PersonalIds.Update(personalId);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.PersonalIds.CountAsync();
    }

    public async Task CreateRangeAsync(IEnumerable<PersonalId> personalIds)
    {
        _context.PersonalIds.AddRange(personalIds);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PersonalId>> GetUnusedAsync(int count)
    {
        return await _context.PersonalIds
            .Where(p => !p.IsUsed)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<PersonalId>> GetAllAsync(int skip, int take)
    {
        return await _context.PersonalIds
            .OrderBy(p => p.Number)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}

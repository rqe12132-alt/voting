using VotingApp.Models;

namespace VotingApp.Repositories;

public interface IPersonalIdRepository
{
    Task<PersonalId?> GetByNumberAsync(string number);
    Task<PersonalId?> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsAsync(string number);
    Task<bool> AnyAsync();
    Task CreateAsync(PersonalId personalId);
    Task UpdateAsync(PersonalId personalId);
    Task<int> CountAsync();
    Task CreateRangeAsync(IEnumerable<PersonalId> personalIds);
    Task<List<PersonalId>> GetUnusedAsync(int count);
    Task<List<PersonalId>> GetAllAsync(int skip, int take);
}

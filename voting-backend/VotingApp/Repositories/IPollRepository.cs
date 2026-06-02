using VotingApp.Models;

namespace VotingApp.Repositories;

public interface IPollRepository
{
    Task<Poll?> GetByIdAsync(Guid id);
    Task<Poll?> GetByIdWithOptionsAsync(Guid id);
    Task<Poll?> GetByIdWithDetailsAsync(Guid id);
    Task<List<Poll>> GetAllAsync();
    Task<List<Poll>> GetActiveAsync();
    Task<List<Poll>> GetActivePagedAsync(int skip, int take, bool includeClosed);
    Task<int> CountActiveAsync(bool includeClosed);
    Task<List<Poll>> GetByStatusAsync(PollStatus status);
    Task<List<Poll>> GetVotedPollsAsync(Guid userId);
    Task<Poll> CreateAsync(Poll poll);
    Task UpdateAsync(Poll poll);
    Task DeleteAsync(Poll poll);
}

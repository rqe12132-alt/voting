using VotingApp.Models;

namespace VotingApp.Repositories;

public interface IVoteRepository
{
    Task<Vote?> GetByUserAndPollAsync(Guid userId, Guid pollId);
    Task<List<Vote>> GetByPollAsync(Guid pollId);
    Task<Vote> CreateAsync(Vote vote);
    Task<int> CountByPollAsync(Guid pollId);
    Task<int> CountByOptionAsync(Guid optionId);
    Task<Dictionary<Guid, int>> GetResultsAsync(Guid pollId);
}

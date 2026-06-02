using VotingApp.DTOs.Vote;
using VotingApp.Models;

namespace VotingApp.Services;

public interface IVoteService
{
    Task<bool> VoteAsync(Guid userId, Guid pollId, VoteRequest request);
    Task<PollResultsResponse?> GetResultsAsync(Guid pollId, Guid? userId = null);
    Task<MyVoteDto?> GetMyVoteAsync(Guid userId, Guid pollId);
}

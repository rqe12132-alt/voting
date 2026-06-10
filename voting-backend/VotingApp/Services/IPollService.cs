using VotingApp.DTOs.Poll;
using VotingApp.Models;

namespace VotingApp.Services;

public interface IPollService
{
    Task<PollResponse?> CreatePollAsync(Guid userId, CreatePollRequest request);
    Task<PollResponse?> UpdatePollAsync(Guid pollId, UpdatePollRequest request);
    Task<bool> PublishPollAsync(Guid pollId);
    Task<bool> ClosePollAsync(Guid pollId);
    Task<bool> ExtendPollAsync(Guid pollId, DateTime? newEndsAt);
    Task<bool> DeletePollAsync(Guid pollId);
    Task<PollResponse?> GetPollByIdAsync(Guid id);
    Task<List<PollListItemDto>> GetActivePollsAsync(Guid? userId = null);
    Task<PagedResult<PollListItemDto>> GetActivePollsPagedAsync(Guid? userId, int page, int pageSize, bool includeClosed);
    Task<List<PollListItemDto>> GetVotedPollsAsync(Guid userId);
    Task<List<PollListItemDto>> GetAllPollsAsync();
    Task<bool> CanEditAsync(Guid pollId);
}

using VotingApp.DTOs.Poll;
using VotingApp.Models;
using VotingApp.Repositories;

namespace VotingApp.Services;

public class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;
    private readonly IVoteRepository _voteRepository;

    public PollService(IPollRepository pollRepository, IVoteRepository voteRepository)
    {
        _pollRepository = pollRepository;
        _voteRepository = voteRepository;
    }

    public async Task<PollResponse?> CreatePollAsync(Guid userId, CreatePollRequest request)
    {
        if (!Enum.TryParse<PollType>(request.Type, out var pollType))
        {
            return null;
        }

        if (!Enum.TryParse<ResultsVisibility>(request.ResultsVisibility, out var visibility))
        {
            visibility = ResultsVisibility.AlwaysVisible;
        }

        var poll = new Poll
        {
            Title = request.Title,
            Description = request.Description,
            Type = pollType,
            Status = PollStatus.Draft,
            ResultsVisibility = visibility,
            IsRealtime = request.IsRealtime,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            ImageUrl = request.ImageUrl,
            CreatedById = userId,
            Options = new List<PollOption>()
        };

        if (pollType == PollType.YesNo)
        {
            poll.Options.Add(new PollOption { Text = "Да", SortOrder = 0 });
            poll.Options.Add(new PollOption { Text = "Нет", SortOrder = 1 });
        }
        else
        {
            for (int i = 0; i < request.Options.Count; i++)
            {
                poll.Options.Add(new PollOption
                {
                    Text = request.Options[i].Text,
                    ImageUrl = request.Options[i].ImageUrl,
                    SortOrder = i
                });
            }
        }

        var created = await _pollRepository.CreateAsync(poll);
        return MapToResponse(created);
    }

    public async Task<PollResponse?> UpdatePollAsync(Guid pollId, UpdatePollRequest request)
    {
        var poll = await _pollRepository.GetByIdWithOptionsAsync(pollId);
        if (poll == null || poll.Status != PollStatus.Draft)
        {
            return null;
        }

        poll.Title = request.Title;
        poll.Description = request.Description;
        poll.ResultsVisibility = Enum.TryParse<ResultsVisibility>(request.ResultsVisibility, out var vis) ? vis : poll.ResultsVisibility;
        poll.IsRealtime = request.IsRealtime;
        poll.StartsAt = request.StartsAt;
        poll.EndsAt = request.EndsAt;
        poll.ImageUrl = request.ImageUrl ?? poll.ImageUrl;
        poll.UpdatedAt = DateTime.UtcNow;

        // Пересоздаем опции только если тип не YesNo (или если переданы новые)
        if (poll.Type != PollType.YesNo && request.Options.Count > 0)
        {
            poll.Options.Clear();
            for (int i = 0; i < request.Options.Count; i++)
            {
                poll.Options.Add(new PollOption
                {
                    Text = request.Options[i].Text,
                    ImageUrl = request.Options[i].ImageUrl,
                    SortOrder = i
                });
            }
        }

        await _pollRepository.UpdateAsync(poll);
        return MapToResponse(poll);
    }

    public async Task<bool> PublishPollAsync(Guid pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null || poll.Status != PollStatus.Draft) return false;

        poll.Status = PollStatus.Active;
        await _pollRepository.UpdateAsync(poll);
        return true;
    }

    public async Task<bool> ClosePollAsync(Guid pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null || poll.Status != PollStatus.Active) return false;

        poll.Status = PollStatus.Closed;
        await _pollRepository.UpdateAsync(poll);
        return true;
    }

    public async Task<bool> ExtendPollAsync(Guid pollId, DateTime? newEndsAt)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null) return false;
        if (poll.Status != PollStatus.Active && poll.Status != PollStatus.Draft) return false;

        poll.EndsAt = newEndsAt;
        await _pollRepository.UpdateAsync(poll);
        return true;
    }

    public async Task<bool> DeletePollAsync(Guid pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll == null) return false;

        await _pollRepository.DeleteAsync(poll);
        return true;
    }

    public async Task<PollResponse?> GetPollByIdAsync(Guid id)
    {
        var poll = await _pollRepository.GetByIdWithOptionsAsync(id);
        return poll == null ? null : MapToResponse(poll);
    }

    public async Task<List<PollListItemDto>> GetActivePollsAsync(Guid? userId = null)
    {
        var polls = await _pollRepository.GetActiveAsync();
        var result = polls.Select(MapToListItem).ToList();

        if (userId.HasValue)
        {
            foreach (var poll in result)
            {
                var vote = await _voteRepository.GetByUserAndPollAsync(userId.Value, poll.Id);
                poll.HasVoted = vote != null;
            }
        }

        return result;
    }

    public async Task<PagedResult<PollListItemDto>> GetActivePollsPagedAsync(Guid? userId, int page, int pageSize, bool includeClosed)
    {
        var skip = (page - 1) * pageSize;
        var polls = await _pollRepository.GetActivePagedAsync(skip, pageSize, includeClosed);
        var total = await _pollRepository.CountActiveAsync(includeClosed);

        var items = polls.Select(MapToListItem).ToList();

        if (userId.HasValue)
        {
            foreach (var poll in items)
            {
                var vote = await _voteRepository.GetByUserAndPollAsync(userId.Value, poll.Id);
                poll.HasVoted = vote != null;
            }
        }

        return new PagedResult<PollListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<PollListItemDto>> GetVotedPollsAsync(Guid userId)
    {
        var polls = await _pollRepository.GetVotedPollsAsync(userId);
        var result = polls.Select(MapToListItem).ToList();

        foreach (var poll in result)
        {
            poll.HasVoted = true;
        }

        return result;
    }

    public async Task<List<PollListItemDto>> GetAllPollsAsync()
    {
        var polls = await _pollRepository.GetAllAsync();
        return polls.Select(MapToListItem).ToList();
    }

    public async Task<bool> CanEditAsync(Guid pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        return poll != null && poll.Status == PollStatus.Draft;
    }

    private static PollResponse MapToResponse(Poll poll)
    {
        return new PollResponse
        {
            Id = poll.Id,
            Title = poll.Title,
            Description = poll.Description,
            Type = poll.Type.ToString(),
            Status = poll.Status.ToString(),
            ResultsVisibility = poll.ResultsVisibility.ToString(),
            IsRealtime = poll.IsRealtime,
            StartsAt = poll.StartsAt,
            EndsAt = poll.EndsAt,
            ImageUrl = poll.ImageUrl,
            CreatedAt = poll.CreatedAt,
            Options = poll.Options.Select(o => new PollOptionDto
            {
                Id = o.Id,
                Text = o.Text,
                ImageUrl = o.ImageUrl,
                SortOrder = o.SortOrder
            }).ToList()
        };
    }

    private static PollListItemDto MapToListItem(Poll poll)
    {
        return new PollListItemDto
        {
            Id = poll.Id,
            Title = poll.Title,
            Type = poll.Type.ToString(),
            Status = poll.Status.ToString(),
            IsRealtime = poll.IsRealtime,
            ImageUrl = poll.ImageUrl,
            EndsAt = poll.EndsAt,
            CreatedAt = poll.CreatedAt
        };
    }
}

using Microsoft.AspNetCore.SignalR;
using VotingApp.DTOs.Vote;
using VotingApp.Hubs;
using VotingApp.Models;
using VotingApp.Repositories;

namespace VotingApp.Services;

public class VoteService : IVoteService
{
    private readonly IVoteRepository _voteRepository;
    private readonly IPollRepository _pollRepository;
    private readonly IHubContext<PollHub> _hubContext;

    public VoteService(IVoteRepository voteRepository, IPollRepository pollRepository, IHubContext<PollHub> hubContext)
    {
        _voteRepository = voteRepository;
        _pollRepository = pollRepository;
        _hubContext = hubContext;
    }

    public async Task<bool> VoteAsync(Guid userId, Guid pollId, VoteRequest request)
    {
        var poll = await _pollRepository.GetByIdWithOptionsAsync(pollId);
        if (poll == null || poll.Status != PollStatus.Active)
        {
            return false;
        }

        // Проверка сроков
        if (poll.StartsAt.HasValue && poll.StartsAt.Value > DateTime.UtcNow) return false;
        if (poll.EndsAt.HasValue && poll.EndsAt.Value < DateTime.UtcNow) return false;

        // Проверка повторного голосования
        var existingVote = await _voteRepository.GetByUserAndPollAsync(userId, pollId);
        if (existingVote != null) return false;

        // Валидация типа
        if (poll.Type == PollType.SingleChoice || poll.Type == PollType.YesNo)
        {
            if (request.OptionIds.Count != 1) return false;
        }

        if (poll.Type == PollType.MultipleChoice)
        {
            if (request.OptionIds.Count < 1) return false;
        }

        // Валидация optionIds принадлежат ли голосованию
        var validOptionIds = poll.Options.Select(o => o.Id).ToHashSet();
        if (!request.OptionIds.All(id => validOptionIds.Contains(id))) return false;

        foreach (var optionId in request.OptionIds)
        {
            var vote = new Vote
            {
                UserId = userId,
                PollId = pollId,
                OptionId = optionId
            };
            await _voteRepository.CreateAsync(vote);
        }

        // Отправка real-time обновления
        if (poll.IsRealtime)
        {
            var results = await GetResultsAsync(pollId);
            if (results != null)
            {
                await _hubContext.Clients.Group(pollId.ToString())
                    .SendAsync("ResultsUpdated", results);
            }
        }

        return true;
    }

    public async Task<PollResultsResponse?> GetResultsAsync(Guid pollId, Guid? userId = null)
    {
        var poll = await _pollRepository.GetByIdWithOptionsAsync(pollId);
        if (poll == null) return null;

        // Проверка видимости результатов
        if (poll.ResultsVisibility == ResultsVisibility.Hidden && poll.Status != PollStatus.Closed)
        {
            return null;
        }

        if (poll.ResultsVisibility == ResultsVisibility.VisibleAfterVote && userId.HasValue)
        {
            var userVote = await _voteRepository.GetByUserAndPollAsync(userId.Value, pollId);
            if (userVote == null && poll.Status != PollStatus.Closed)
            {
                return null;
            }
        }

        var counts = await _voteRepository.GetResultsAsync(pollId);
        var total = counts.Values.Sum();

        var results = poll.Options.Select(o =>
        {
            var count = counts.ContainsKey(o.Id) ? counts[o.Id] : 0;
            var percentage = total > 0 ? Math.Round((double)count / total * 100, 2) : 0;
            return new VoteResultDto
            {
                OptionId = o.Id,
                OptionText = o.Text,
                Count = count,
                Percentage = percentage
            };
        }).ToList();

        return new PollResultsResponse
        {
            PollId = pollId,
            Title = poll.Title,
            TotalVotes = total,
            Results = results
        };
    }

    public async Task<MyVoteDto?> GetMyVoteAsync(Guid userId, Guid pollId)
    {
        var vote = await _voteRepository.GetByUserAndPollAsync(userId, pollId);
        if (vote == null) return null;

        // Для multiple choice нужно вернуть все голоса пользователя
        var allVotes = await _voteRepository.GetByPollAsync(pollId);
        var myVotes = allVotes.Where(v => v.UserId == userId).Select(v => v.OptionId).ToList();

        return new MyVoteDto
        {
            PollId = pollId,
            OptionIds = myVotes,
            VotedAt = vote.CreatedAt
        };
    }
}
